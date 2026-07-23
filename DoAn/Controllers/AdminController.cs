using System.Security.Claims;
using DoAn.Data;
using DoAn.Models;
using DoAn.Models.Enums;
using DoAn.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _context.Users.CountAsync(),

            TotalPosts = await _context.Posts
                .CountAsync(post => !post.IsDeleted),

            TotalComments = await _context.Comments
                .CountAsync(comment => !comment.IsDeleted),

            TotalAdmins = await _context.Users
                .CountAsync(user => user.Role == UserRole.Admin)
        };

        return View(model);
    }

    // =========================================================
    // USERS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.FullName)
            .ToListAsync();

        return View(users);
    }

    // =========================================================
    // USER DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> UserDetails(int id)
    {
        // AsNoTracking giúp luôn lấy trạng thái mới nhất.
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        ViewBag.PostCount = await _context.Posts
            .CountAsync(post =>
                post.UserId == id &&
                !post.IsDeleted);

        ViewBag.CommentCount = await _context.Comments
            .CountAsync(comment =>
                comment.UserId == id &&
                !comment.IsDeleted);

        ViewBag.LikeCount = await _context.Likes
            .CountAsync(like => like.UserId == id);

        return View(user);
    }

    // =========================================================
    // EDIT USER GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Users));
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            Bio = user.Bio,
            Role = user.Role,
            Status = user.Status
        };

        return View(model);
    }

    // =========================================================
    // EDIT USER POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Users));
        }

        string username = model.Username.Trim();
        string email = model.Email.Trim().ToLowerInvariant();

        bool usernameExists = await _context.Users.AnyAsync(x =>
            x.Id != model.Id &&
            x.Username == username);

        if (usernameExists)
        {
            ModelState.AddModelError(
                nameof(model.Username),
                "Tên đăng nhập đã được sử dụng.");

            return View(model);
        }

        bool emailExists = await _context.Users.AnyAsync(x =>
            x.Id != model.Id &&
            x.Email == email);

        if (emailExists)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Email đã được sử dụng.");

            return View(model);
        }

        int currentAdminId = GetCurrentUserId();

        if (user.Id == currentAdminId)
        {
            if (model.Role != UserRole.Admin)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Bạn không thể tự hạ quyền quản trị.");

                return View(model);
            }

            if (model.Status != UserStatus.Active)
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Bạn không thể tự khóa tài khoản đang đăng nhập.");

                return View(model);
            }
        }

        user.Username = username;
        user.Email = email;
        user.FullName = model.FullName.Trim();

        user.Phone = string.IsNullOrWhiteSpace(model.Phone)
            ? null
            : model.Phone.Trim();

        user.AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl)
            ? null
            : model.AvatarUrl.Trim();

        user.CoverUrl = string.IsNullOrWhiteSpace(model.CoverUrl)
            ? null
            : model.CoverUrl.Trim();

        user.Bio = string.IsNullOrWhiteSpace(model.Bio)
            ? null
            : model.Bio.Trim();

        user.Role = model.Role;
        user.Status = model.Status;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Cập nhật người dùng thành công.";

        return RedirectToAction(
            nameof(UserDetails),
            new { id = user.Id });
    }

    // =========================================================
    // SUSPEND USER
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendUser(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        if (user.Id == GetCurrentUserId())
        {
            TempData["ErrorMessage"] =
                "Bạn không thể khóa chính tài khoản đang đăng nhập.";

            return RedirectToSafePage(id);
        }

        user.Status = UserStatus.Suspended;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã tạm khóa tài khoản {user.Username}.";

        return RedirectToSafePage(id);
    }

    // =========================================================
    // BAN USER
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        if (user.Id == GetCurrentUserId())
        {
            TempData["ErrorMessage"] =
                "Bạn không thể cấm chính tài khoản đang đăng nhập.";

            return RedirectToSafePage(id);
        }

        user.Status = UserStatus.Banned;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã cấm tài khoản {user.Username}.";

        return RedirectToSafePage(id);
    }

    // =========================================================
    // ACTIVATE USER
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        user.Status = UserStatus.Active;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã kích hoạt lại tài khoản {user.Username}.";

        return RedirectToSafePage(id);
    }

    // =========================================================
    // DELETE USER
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy tài khoản.";

            return RedirectToAction(nameof(Users));
        }

        if (user.Id == GetCurrentUserId())
        {
            TempData["ErrorMessage"] =
                "Bạn không thể xóa chính tài khoản đang đăng nhập.";

            return RedirectToAction(nameof(Users));
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var postIds = await _context.Posts
                .Where(post => post.UserId == id)
                .Select(post => post.Id)
                .ToListAsync();

            // Lời mời kết bạn.
            var requests = await _context.FriendRequests
                .Where(request =>
                    request.SenderId == id ||
                    request.ReceiverId == id)
                .ToListAsync();

            _context.FriendRequests.RemoveRange(requests);

            // Quan hệ bạn bè.
            var friends = await _context.Friends
                .Where(friend =>
                    friend.UserId == id ||
                    friend.FriendId == id)
                .ToListAsync();

            _context.Friends.RemoveRange(friends);

            // Thông báo nhận hoặc tạo bởi tài khoản.
            var userNotifications = await _context.Notifications
                .Where(notification =>
                    notification.UserId == id ||
                    notification.ActorUserId == id)
                .ToListAsync();

            _context.Notifications.RemoveRange(userNotifications);

            if (postIds.Count > 0)
            {
                // Thông báo tham chiếu bài viết.
                var postNotifications =
                    await _context.Notifications
                        .Where(notification =>
                            notification.ReferenceId.HasValue &&
                            postIds.Contains(
                                notification.ReferenceId.Value) &&
                            (notification.Type == "Like" ||
                             notification.Type == "Comment"))
                        .ToListAsync();

                _context.Notifications.RemoveRange(
                    postNotifications);

                // Like trong bài viết.
                var postLikes = await _context.Likes
                    .Where(like =>
                        postIds.Contains(like.PostId))
                    .ToListAsync();

                _context.Likes.RemoveRange(postLikes);

                // Bình luận trong bài viết.
                var postComments = await _context.Comments
                    .Where(comment =>
                        postIds.Contains(comment.PostId))
                    .ToListAsync();

                _context.Comments.RemoveRange(postComments);

                // Media bài viết.
                var media = await _context.PostMedia
                    .Where(item =>
                        postIds.Contains(item.PostId))
                    .ToListAsync();

                _context.PostMedia.RemoveRange(media);
            }

            // Like do tài khoản thực hiện trên bài người khác.
            var userLikes = await _context.Likes
                .Where(like => like.UserId == id)
                .ToListAsync();

            _context.Likes.RemoveRange(userLikes);

            // Bình luận do tài khoản thực hiện.
            var userComments = await _context.Comments
                .Where(comment => comment.UserId == id)
                .ToListAsync();

            _context.Comments.RemoveRange(userComments);

            // Bài viết.
            var posts = await _context.Posts
                .Where(post => post.UserId == id)
                .ToListAsync();

            _context.Posts.RemoveRange(posts);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Đã xóa tài khoản và toàn bộ dữ liệu liên quan.";
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();

            TempData["ErrorMessage"] =
                $"Không thể xóa tài khoản: {exception.GetBaseException().Message}";
        }

        return RedirectToAction(nameof(Users));
    }

    // =========================================================
    // POSTS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Posts()
    {
        var posts = await _context.Posts
            .AsNoTracking()
            .Include(post => post.User)
            .Include(post => post.MediaItems)
            .OrderByDescending(post => post.CreatedAt)
            .ToListAsync();

        return View(posts);
    }

    // =========================================================
    // POST DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> PostDetails(int id)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.MediaItems)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        ViewBag.LikeCount = await _context.Likes
            .CountAsync(like => like.PostId == id);

        ViewBag.CommentCount = await _context.Comments
            .CountAsync(comment =>
                comment.PostId == id &&
                !comment.IsDeleted);

        return View(post);
    }

    // =========================================================
    // EDIT POST GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        var model = new EditPostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            IsHidden = post.IsHidden,
            IsDeleted = post.IsDeleted
        };

        return View(model);
    }

    // =========================================================
    // EDIT POST POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(
        EditPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(item => item.Id == model.Id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        post.Content = model.Content.Trim();
        post.IsHidden = model.IsHidden;
        post.IsDeleted = model.IsDeleted;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật bài viết.";

        return RedirectToAction(
            nameof(PostDetails),
            new { id = post.Id });
    }

    // =========================================================
    // HIDE POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HidePost(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        if (post.IsDeleted)
        {
            TempData["ErrorMessage"] =
                "Không thể ẩn bài viết đã bị xóa.";

            return RedirectToAction(nameof(Posts));
        }

        post.IsHidden = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã ẩn bài viết.";

        return RedirectToAction(nameof(Posts));
    }

    // =========================================================
    // SHOW POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShowPost(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        if (post.IsDeleted)
        {
            TempData["ErrorMessage"] =
                "Hãy khôi phục bài viết trước khi hiển thị.";

            return RedirectToAction(nameof(Posts));
        }

        post.IsHidden = false;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã hiển thị lại bài viết.";

        return RedirectToAction(nameof(Posts));
    }

    // =========================================================
    // SOFT DELETE POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        post.IsDeleted = true;
        post.IsHidden = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã chuyển bài viết sang trạng thái đã xóa.";

        return RedirectToAction(nameof(Posts));
    }

    // =========================================================
    // RESTORE POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestorePost(int id)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(item => item.Id == id);

        if (post == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy bài viết.";

            return RedirectToAction(nameof(Posts));
        }

        post.IsDeleted = false;
        post.IsHidden = false;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã khôi phục bài viết.";

        return RedirectToAction(nameof(Posts));
    }
    // =========================================================
    // COMMENTS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Comments()
    {
        var comments = await _context.Comments
            .AsNoTracking()
            .OrderByDescending(comment => comment.CreatedAt)
            .ToListAsync();

        var userIds = comments
            .Select(comment => comment.UserId)
            .Distinct()
            .ToList();

        var postIds = comments
            .Select(comment => comment.PostId)
            .Distinct()
            .ToList();

        ViewBag.Users = await _context.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id);

        ViewBag.Posts = await _context.Posts
            .AsNoTracking()
            .Where(post => postIds.Contains(post.Id))
            .ToDictionaryAsync(post => post.Id);

        return View(comments);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private int GetCurrentUserId()
    {
        string? value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out int userId))
        {
            throw new InvalidOperationException(
                "Không xác định được tài khoản quản trị.");
        }

        return userId;
    }

    private IActionResult RedirectToSafePage(int userId)
    {
        string? referer = Request.Headers.Referer;

        // Nếu thao tác từ trang Details thì quay lại Details.
        if (!string.IsNullOrWhiteSpace(referer) &&
            referer.Contains(
                "/Admin/UserDetails",
                StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(
                nameof(UserDetails),
                new { id = userId });
        }

        return RedirectToAction(nameof(Users));
    }
}