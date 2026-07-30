using System.Security.Claims;
using DoAn.Models;
using DoAn.Services;
using DoAn.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers;

[Authorize]//đăng nhập mới đc sử dụng
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
    public IActionResult Create() => View(new CreatePostViewModel());//mở giao diện tạo bài

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        model.MediaFiles ??= new List<IFormFile>();//??= nghĩa là nếu mediaf đg null thì gán danh sách rỗng
        var files = model.MediaFiles.Where(x => x.Length > 0).ToList();

        ValidateMediaCollection(files, 0);
        if (!ModelState.IsValid) return View(model);
        //tạo post
        var post = new Post
        {
            UserId = GetCurrentUserId(),
            Content = model.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsHidden = false
        };
        //lưu file
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
        //Lưu bài
        await _postService.CreateAsync(post);
        TempData["SuccessMessage"] = "Đăng bài viết thành công.";
        return RedirectToAction("Index", "Home");//chuyển về home
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)// chi tiết bài đăng
    {
        if (id <= 0) return BadRequest();

        var model = await _postService.GetDetailsAsync(id, GetCurrentUserId());
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]//lấy bài viết sau đó chuyển sang view
    public async Task<IActionResult> MyPosts()
    {
        var posts = await _postService.GetMyPostsAsync(GetCurrentUserId());
        return View(posts);
    }

    [HttpGet]//lấy bài viết để sửa
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
        // Lấy ID của người đang đăng nhập từ cookie authentication.
        var userId = GetCurrentUserId();

        // Lấy đúng bài viết thuộc về người đang đăng nhập.
        // Entity Post được EF Core tracking để chỉ cập nhật các trường thay đổi.
        var post = await _postService.GetForEditAsync(model.Id, userId);
        if (post == null)
        {
            return NotFound();
        }

        // Tránh lỗi null khi form không gửi file hoặc không chọn media cần xóa.
        model.MediaFiles ??= new List<IFormFile>();
        model.RemoveMediaIds ??= new List<int>();

        // Chỉ lấy những media thật sự thuộc bài viết hiện tại.
        // Người dùng không thể gửi ID media của bài khác để xóa.
        var removableMedia = post.MediaItems
            .Where(media => model.RemoveMediaIds.Contains(media.Id))
            .ToList();

        // Chỉ xử lý các file có dữ liệu.
        var newFiles = model.MediaFiles
            .Where(file => file.Length > 0)
            .ToList();

        // Kiểm tra tổng số media sau khi xóa cũ và thêm mới.
        var remainingMediaCount =
            post.MediaItems.Count - removableMedia.Count;

        ValidateMediaCollection(newFiles, remainingMediaCount);

        if (!ModelState.IsValid)
        {
            // Khi validation lỗi, nạp lại danh sách media cũ để View hiển thị.
            model.CurrentMedia = post.MediaItems
                .OrderBy(media => media.DisplayOrder)
                .Select(media => new PostMediaViewModel
                {
                    Id = media.Id,
                    MediaUrl = media.MediaUrl,
                    MediaType = media.MediaType,
                    DisplayOrder = media.DisplayOrder
                })
                .ToList();

            return View(model);
        }

        // Chỉ cập nhật nội dung của Post hiện tại.
        // Không tạo Post mới nên PostId không thay đổi. Vì PostId giữ nguyên,
        // toàn bộ Likes và Comments trước đó vẫn liên kết với bài viết này.
        post.Content = model.Content.Trim();

        // Xóa những PostMedia được người dùng chọn.
        // Thao tác này chỉ tác động bảng PostMedia, không tác động Likes/Comments.
        foreach (var media in removableMedia)
        {
            DeleteLocalMediaFile(media.MediaUrl);
            post.MediaItems.Remove(media);
        }

        // Xác định thứ tự bắt đầu cho các media mới.
        var nextDisplayOrder = post.MediaItems.Any()
            ? post.MediaItems.Max(media => media.DisplayOrder) + 1
            : 0;

        // Lưu file mới và thêm các dòng PostMedia mới vào cùng PostId.
        foreach (var file in newFiles)
        {
            var savedMedia = await SaveMediaAsync(file);

            post.MediaItems.Add(new PostMedia
            {
                MediaUrl = savedMedia.Url,
                MediaType = savedMedia.Type,
                DisplayOrder = nextDisplayOrder++,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Sắp xếp lại DisplayOrder sau khi xóa hoặc thêm media.
        var orderedMedia = post.MediaItems
            .OrderBy(media => media.DisplayOrder)
            .ToList();

        for (var index = 0; index < orderedMedia.Count; index++)
        {
            orderedMedia[index].DisplayOrder = index;
        }

        // SaveChangesAsync chỉ lưu Content, UpdatedAt và PostMedia thay đổi.
        // Không có lệnh Remove đối với bảng Likes hoặc Comments.
        await _postService.UpdateAsync(post);

        TempData["SuccessMessage"] =
            "Cập nhật bài viết thành công. Lượt thích và bình luận được giữ nguyên.";

        return RedirectToAction(
            nameof(Details),
            new { id = post.Id });
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
        //like hoặc unlike
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
