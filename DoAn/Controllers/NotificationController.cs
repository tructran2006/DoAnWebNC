using System.Security.Claims;
using DoAn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(
        NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();

        var notifications =
            await _notificationService
                .GetUserNotificationsAsync(userId);

        ViewBag.UnreadCount =
            await _notificationService
                .GetUnreadCountAsync(userId);

        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> Read(int id)
    {
        int userId = GetCurrentUserId();

        var notifications =
            await _notificationService
                .GetUserNotificationsAsync(userId);

        var notification =
            notifications.FirstOrDefault(x => x.Id == id);

        if (notification == null)
        {
            return NotFound();
        }

        await _notificationService.MarkAsReadAsync(
            id,
            userId);

        if (!string.IsNullOrWhiteSpace(notification.Url) &&
            Url.IsLocalUrl(notification.Url))
        {
            return LocalRedirect(notification.Url);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(
            GetCurrentUserId());

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _notificationService.DeleteAsync(
            id,
            GetCurrentUserId());

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        await _notificationService.DeleteAllAsync(
            GetCurrentUserId());

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        int count =
            await _notificationService
                .GetUnreadCountAsync(GetCurrentUserId());

        return Json(new
        {
            count
        });
    }

    private int GetCurrentUserId()
    {
        string? value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out int userId))
        {
            throw new InvalidOperationException(
                "Không xác định được người dùng đang đăng nhập.");
        }

        return userId;
    }
}