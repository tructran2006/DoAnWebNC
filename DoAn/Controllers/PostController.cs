using System.Security.Claims;
using DoAn.Models;
using DoAn.Services;
using DoAn.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly PostService _postService;
    private readonly NotificationService _notificationService;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    private static readonly string[] AllowedImageExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private static readonly string[] AllowedVideoExtensions =
        [".mp4", ".webm", ".mov"];

    private const long MaxMediaSize = 50 * 1024 * 1024;
    private const int MaxMediaCount = 10;

    public PostController(
        PostService postService,
        NotificationService notificationService,
        UserService userService,
        IWebHostEnvironment environment)
    {
        _postService = postService;
        _notificationService = notificationService;
        _userService = userService;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Create() => View(new CreatePostViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        model.MediaFiles ??= new List<IFormFile>();
        var files = model.MediaFiles.Where(x => x.Length > 0).ToList();

        ValidateMediaCollection(files, 0);
        if (!ModelState.IsValid) return View(model);

        var post = new Post
        {
            UserId = GetCurrentUserId(),
            Content = model.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsHidden = false
        };

        for (var index = 0; index < files.Count; index++)
        {
            var saved = await SaveMediaAsync(files[index]);
            post.MediaItems.Add(new PostMedia
            {
                MediaUrl = saved.Url,
                MediaType = saved.Type,
                DisplayOrder = index,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _postService.CreateAsync(post);
        TempData["SuccessMessage"] = "Đăng bài viết thành công.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0) return BadRequest();

        var model = await _postService.GetDetailsAsync(id, GetCurrentUserId());
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MyPosts()
    {
        var posts = await _postService.GetMyPostsAsync(GetCurrentUserId());
        return View(posts);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _postService.GetForEditAsync(id, GetCurrentUserId());
        if (post == null) return NotFound();

        return View(new EditPostViewModel
        {
            Id = post.Id,
            Content = post.Content,
            CurrentMedia = post.MediaItems
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PostMediaViewModel
                {
                    Id = x.Id,
                    MediaUrl = x.MediaUrl,
                    MediaType = x.MediaType,
                    DisplayOrder = x.DisplayOrder
                }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPostViewModel model)
    {
        var userId = GetCurrentUserId();
        var post = await _postService.GetForEditAsync(model.Id, userId);
        if (post == null) return NotFound();

        model.MediaFiles ??= new List<IFormFile>();
        model.RemoveMediaIds ??= new List<int>();

        var removable = post.MediaItems
            .Where(x => model.RemoveMediaIds.Contains(x.Id))
            .ToList();

        var newFiles = model.MediaFiles.Where(x => x.Length > 0).ToList();
        ValidateMediaCollection(newFiles, post.MediaItems.Count - removable.Count);

        if (!ModelState.IsValid)
        {
            model.CurrentMedia = post.MediaItems
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PostMediaViewModel
                {
                    Id = x.Id,
                    MediaUrl = x.MediaUrl,
                    MediaType = x.MediaType,
                    DisplayOrder = x.DisplayOrder
                }).ToList();
            return View(model);
        }

        post.Content = model.Content.Trim();

        foreach (var media in removable)
        {
            DeleteLocalMediaFile(media.MediaUrl);
            post.MediaItems.Remove(media);
        }

        var nextOrder = post.MediaItems.Any()
            ? post.MediaItems.Max(x => x.DisplayOrder) + 1
            : 0;

        foreach (var file in newFiles)
        {
            var saved = await SaveMediaAsync(file);
            post.MediaItems.Add(new PostMedia
            {
                MediaUrl = saved.Url,
                MediaType = saved.Type,
                DisplayOrder = nextOrder++,
                CreatedAt = DateTime.UtcNow
            });
        }

        var ordered = post.MediaItems.OrderBy(x => x.DisplayOrder).ToList();
        for (var i = 0; i < ordered.Count; i++) ordered[i].DisplayOrder = i;

        await _postService.UpdateAsync(post);
        TempData["SuccessMessage"] = "Cập nhật bài viết thành công.";
        return RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnPage = null)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post == null) return NotFound();
        if (post.UserId != GetCurrentUserId()) return Forbid();

        await _postService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Xóa bài viết thành công.";

        return string.Equals(returnPage, "Home", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Home")
            : RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLikeAjax([FromBody] LikeRequest model)
    {
        if (model.PostId <= 0)
            return BadRequest(new { success = false, message = "Bài viết không hợp lệ." });

        var userId = GetCurrentUserId();
        var post = await _postService.GetByIdAsync(model.PostId);
        if (post == null || post.IsHidden)
            return NotFound(new { success = false, message = "Không tìm thấy bài viết." });

        await _postService.ToggleLikeAsync(model.PostId, userId);
        var liked = await _postService.IsLikedAsync(model.PostId, userId);
        var likeCount = await _postService.GetLikeCountAsync(model.PostId);

        if (liked && post.UserId != userId)
        {
            var actor = await _userService.GetByIdAsync(userId);
            var actorName = string.IsNullOrWhiteSpace(actor?.FullName)
                ? "Một người dùng"
                : actor.FullName;

            await _notificationService.CreateOrRefreshAsync(
                post.UserId,
                userId,
                "Like",
                post.Id,
                $"{actorName} đã thích bài viết của bạn.",
                $"/Post/Details/{post.Id}");
        }

        return Json(new { success = true, liked, likeCount });
    }

    private void ValidateMediaCollection(IReadOnlyCollection<IFormFile> files, int existingCount)
    {
        if (existingCount + files.Count > MaxMediaCount)
        {
            ModelState.AddModelError("MediaFiles", $"Mỗi bài viết chỉ được tối đa {MaxMediaCount} ảnh hoặc video.");
        }

        foreach (var file in files)
        {
            var error = ValidateMediaFile(file);
            if (error != null) ModelState.AddModelError("MediaFiles", error);
        }
    }

    private string? ValidateMediaFile(IFormFile file)
    {
        if (file.Length <= 0) return "File tải lên không hợp lệ.";
        if (file.Length > MaxMediaSize) return $"File {file.FileName} vượt quá 50 MB.";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                      && AllowedImageExtensions.Contains(extension);
        var isVideo = file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                      && AllowedVideoExtensions.Contains(extension);

        return !isImage && !isVideo
            ? $"File {file.FileName} không đúng định dạng hỗ trợ."
            : null;
    }

    private async Task<(string Url, string Type)> SaveMediaAsync(IFormFile file)
    {
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
        Directory.CreateDirectory(folder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await file.CopyToAsync(stream);

        var type = file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? "video"
            : "image";

        return ($"/uploads/posts/{fileName}", type);
    }

    private void DeleteLocalMediaFile(string? mediaUrl)
    {
        if (string.IsNullOrWhiteSpace(mediaUrl) ||
            !mediaUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return;

        var relative = mediaUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var path = Path.Combine(_environment.WebRootPath, relative);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId))
            throw new InvalidOperationException("Không xác định được người dùng đang đăng nhập.");
        return userId;
    }
}
