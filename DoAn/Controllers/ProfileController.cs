using System.Security.Claims;
using DoAn.Services;
using DoAn.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private readonly UserService _userService;
    private readonly PostService _postService;
    private readonly FriendService _friendService;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        UserService userService,
        PostService postService,
        FriendService friendService,
        IWebHostEnvironment environment)
    {
        _userService = userService;
        _postService = postService;
        _friendService = friendService;
        _environment = environment;
    }

    public async Task<IActionResult> Index(int? id)
    {
        int currentUserId = GetCurrentUserId();
        int profileId = id ?? currentUserId;
        var user = await _userService.GetByIdAsync(profileId);

        if (user == null)
            return NotFound();

        bool isOwnProfile = profileId == currentUserId;
        var model = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Bio = user.Bio ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            CreatedAt = user.CreatedAt,
            FriendCount = (await _friendService.GetFriendsAsync(profileId)).Count,
            Posts = await _postService.GetProfilePostsAsync(profileId, currentUserId),
            IsOwnProfile = isOwnProfile
        };

        model.PostCount = model.Posts.Count;

        if (!isOwnProfile)
        {
            model.IsFriend = await _friendService.AreFriendsAsync(currentUserId, profileId);
            var sentRequest = await _friendService.GetPendingRequestAsync(currentUserId, profileId);
            var receivedRequest = await _friendService.GetPendingRequestAsync(profileId, currentUserId);
            model.HasSentFriendRequest = sentRequest != null;
            model.HasReceivedFriendRequest = receivedRequest != null;
            model.ReceivedRequestId = receivedRequest?.Id;
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userService.GetByIdAsync(GetCurrentUserId());
        if (user == null)
            return NotFound();

        return View(new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Bio = user.Bio,
            CurrentAvatar = user.AvatarUrl,
            CurrentCover = user.CoverUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        int userId = GetCurrentUserId();
        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
            return NotFound();

        model.CurrentAvatar = user.AvatarUrl;
        model.CurrentCover = user.CoverUrl;

        string normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _userService.ExistsByEmailExceptAsync(normalizedEmail, userId))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
        }

        ValidateImage(model.AvatarFile, nameof(model.AvatarFile), "ảnh đại diện");
        ValidateImage(model.CoverFile, nameof(model.CoverFile), "ảnh bìa");

        if (!ModelState.IsValid)
            return View(model);

        user.FullName = model.FullName.Trim();
        user.Email = normalizedEmail;
        user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();

        if (model.AvatarFile is { Length: > 0 })
        {
            user.AvatarUrl = await SaveImageAsync(model.AvatarFile, "avatars");
        }

        if (model.CoverFile is { Length: > 0 })
        {
            user.CoverUrl = await SaveImageAsync(model.CoverFile, "covers");
        }

        await _userService.UpdateAsync(user);

        TempData["SuccessMessage"] = "Hồ sơ đã được cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Friends(int? id)
    {
        int currentUserId = GetCurrentUserId();
        int profileId = id ?? currentUserId;
        var user = await _userService.GetByIdAsync(profileId);

        if (user == null)
            return NotFound();

        var relationships = await _friendService.GetFriendsAsync(profileId);
        var users = await _userService.GetByIdsAsync(relationships.Select(x => x.FriendId));

        var model = new ProfileFriendsViewModel
        {
            ProfileId = profileId,
            ProfileName = user.FullName,
            IsOwnProfile = profileId == currentUserId,
            Friends = users.Select(x => new UserCardViewModel
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                AvatarUrl = x.AvatarUrl,
                Bio = x.Bio ?? string.Empty,
                IsFriend = true
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Photos(int? id)
    {
        int profileId = id ?? GetCurrentUserId();
        var user = await _userService.GetByIdAsync(profileId);

        if (user == null)
            return NotFound();

        var posts = await _postService.GetUserPostsAsync(profileId);
        var model = new ProfilePhotosViewModel
        {
            ProfileId = profileId,
            ProfileName = user.FullName,
            Photos = posts
                .SelectMany(post => post.MediaItems
                    .Where(media => media.MediaType == "image")
                    .Select(media => new ProfilePhotoItemViewModel
                    {
                        PostId = post.Id,
                        ImageUrl = media.MediaUrl,
                        CreatedAt = post.CreatedAt
                    }))
                .OrderByDescending(photo => photo.CreatedAt)
                .ToList()
        };

        return View(model);
    }

    private void ValidateImage(IFormFile? file, string fieldName, string label)
    {
        if (file == null || file.Length == 0)
            return;

        if (file.Length > MaxImageSize)
        {
            ModelState.AddModelError(fieldName, $"{label} không được vượt quá 5 MB.");
            return;
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        bool isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        if (!isImage || !AllowedImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(fieldName, $"{label} chỉ chấp nhận JPG, JPEG, PNG, WEBP hoặc GIF.");
        }
    }

    private async Task<string> SaveImageAsync(IFormFile file, string folderName)
    {
        string webRoot = _environment.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        string uploadsFolder = Path.Combine(webRoot, "uploads", folderName);
        Directory.CreateDirectory(uploadsFolder);

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string fileName = $"{Guid.NewGuid():N}{extension}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    private int GetCurrentUserId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out int userId))
            throw new InvalidOperationException("Không xác định được người dùng đang đăng nhập.");

        return userId;
    }
}
