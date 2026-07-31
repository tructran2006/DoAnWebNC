using DoAn.Data;
using DoAn.Services;
using DoAn.ViewModels;
using DoAn.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn.Controllers;


[Authorize]

public class FriendController : Controller
{
    private readonly FriendService _friendService;
    private readonly AppDbContext _context;

    private readonly NotificationService _notificationService;

    public FriendController(
        FriendService friendService,
        AppDbContext context,
        NotificationService notificationService)
    {
        _friendService = friendService;
        _context = context;
        _notificationService = notificationService;
    }

    // ==========================
    // DANH SÁCH BẠN BÈ
    // ==========================

    [HttpGet]
    public async Task<IActionResult> Friends()
    {
        int currentUserId = GetCurrentUserId();

        var friendIds = await _context.Friends
            .AsNoTracking()
            .Where(item => item.UserId == currentUserId)
            .Select(item => item.FriendId)
            .Distinct()
            .ToListAsync();

        var friends = await _context.Users
            .AsNoTracking()
            .Where(user =>
                friendIds.Contains(user.Id) &&
                user.Status == UserStatus.Active)
            .OrderBy(user => user.FullName)
            .ToListAsync();

        var requests = await _context.FriendRequests
            .AsNoTracking()
            .Where(request =>
                request.ReceiverId == currentUserId &&
                !request.IsAccepted &&
                !request.IsRejected)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        var requestSenderIds = requests
            .Select(request => request.SenderId)
            .Distinct()
            .ToList();

        ViewBag.RequestUsers = await _context.Users
            .AsNoTracking()
            .Where(user => requestSenderIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id);

        var pendingUserIds = await _context.FriendRequests
            .AsNoTracking()
            .Where(request =>
                !request.IsAccepted &&
                !request.IsRejected &&
                (request.SenderId == currentUserId ||
                 request.ReceiverId == currentUserId))
            .Select(request =>
                request.SenderId == currentUserId
                    ? request.ReceiverId
                    : request.SenderId)
            .Distinct()
            .ToListAsync();

        var suggestedUsers = await _context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id != currentUserId &&
                user.Status == UserStatus.Active &&
                !friendIds.Contains(user.Id) &&
                !pendingUserIds.Contains(user.Id))
            .OrderByDescending(user => user.CreatedAt)
            .Take(12)
            .ToListAsync();

        var model = new FriendPageViewModel
        {
            Friends = friends,
            Requests = requests,
            SuggestedUsers = suggestedUsers
        };

        return View(model);
    }

    // ==========================
    // LỜI MỜI KẾT BẠN
    // ==========================

    [HttpGet]
    public IActionResult Requests()
    {
        return RedirectToAction(nameof(Friends));
    }

    // ==========================
    // GỬI LỜI MỜI
    // ==========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int receiverId)
    {
        var currentUserId = GetCurrentUserId();

        if (receiverId != currentUserId)
        {
            await _friendService.SendRequestAsync(
                currentUserId,
                receiverId);

            var sender = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId);

            string senderName =
                string.IsNullOrWhiteSpace(sender?.FullName)
                    ? "Một người dùng"
                    : sender.FullName;

            await _notificationService.CreateOrRefreshAsync(
                receiverId: receiverId,
                actorUserId: currentUserId,
                type: "FriendRequest",
                referenceId: currentUserId,
                message: $"{senderName} đã gửi lời mời kết bạn.",
                url: "/Friend/Friends");
        }

        return RedirectToAction(
            "Index",
            "Profile",
            new { id = receiverId });
    }

    // ==========================
    // CHẤP NHẬN
    // ==========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var request =
            await _friendService.GetRequestByIdAsync(id);

        if (request == null)
            return NotFound();

        if (request.ReceiverId != GetCurrentUserId())
            return Forbid();

        await _friendService.AcceptRequestAsync(id);
        var receiver = await _context.Users
    .AsNoTracking()
    .FirstOrDefaultAsync(
        x => x.Id == request.ReceiverId);

        string receiverName =
            string.IsNullOrWhiteSpace(receiver?.FullName)
                ? "Một người dùng"
                : receiver.FullName;

        await _notificationService.CreateOrRefreshAsync(
            receiverId: request.SenderId,
            actorUserId: request.ReceiverId,
            type: "FriendAccepted",
            referenceId: request.ReceiverId,
            message: $"{receiverName} đã chấp nhận lời mời kết bạn của bạn.",
            url: $"/Profile/Index/{request.ReceiverId}");

        return RedirectToAction(nameof(Friends));
    }

    // ==========================
    // TỪ CHỐI
    // ==========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var request =
            await _friendService.GetRequestByIdAsync(id);

        if (request == null)
            return NotFound();

        if (request.ReceiverId != GetCurrentUserId())
            return Forbid();

        await _friendService.RejectRequestAsync(id);

        return RedirectToAction(nameof(Friends));
    }

    // ==========================
    // HỦY KẾT BẠN
    // ==========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int friendId)
    {
        await _friendService.RemoveFriendAsync(
            GetCurrentUserId(),
            friendId);

        return RedirectToAction(nameof(Friends));
    }

    // ==========================
    // USER ID
    // ==========================

    private int GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out int userId))
        {
            throw new InvalidOperationException(
                "Không xác định được người dùng.");
        }

        return userId;
    }
}