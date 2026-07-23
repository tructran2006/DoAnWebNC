using System.Security.Claims;
using DoAn.Data;
using DoAn.Models.Enums;
using DoAn.ViewModels.Post;
using DoAn.ViewModels.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q)
    {
        string keyword =
            q?.Trim() ?? string.Empty;

        var model = new SearchViewModel
        {
            Query = keyword
        };

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return View(model);
        }

        int currentUserId =
            GetCurrentUserId();

        string normalizedKeyword =
            keyword.ToLower();

        model.Users = await _context.Users
            .AsNoTracking()
            .Where(user =>
                user.Status == UserStatus.Active &&
                (
                    user.FullName.ToLower()
                        .Contains(normalizedKeyword) ||

                    user.Username.ToLower()
                        .Contains(normalizedKeyword) ||

                    user.Email.ToLower()
                        .Contains(normalizedKeyword)
                ))
            .OrderBy(user => user.FullName)
            .Take(20)
            .Select(user =>
                new SearchUserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    Bio = user.Bio
                })
            .ToListAsync();

        model.Posts = await (
            from post in _context.Posts
            join user in _context.Users
                on post.UserId equals user.Id

            where !post.IsDeleted &&
                  !post.IsHidden &&
                  post.Content.ToLower()
                      .Contains(normalizedKeyword)

            orderby post.CreatedAt descending

            select new FeedPostViewModel
            {
                Id = post.Id,
                UserId = post.UserId,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,

                LikeCount = _context.Likes.Count(
                    like => like.PostId == post.Id),

                CommentCount = _context.Comments.Count(
                    comment =>
                        comment.PostId == post.Id &&
                        !comment.IsDeleted),

                IsLikedByCurrentUser =
                    _context.Likes.Any(
                        like =>
                            like.PostId == post.Id &&
                            like.UserId == currentUserId)
            })
            .Take(30)
            .ToListAsync();

        return View(model);
    }

    private int GetCurrentUserId()
    {
        string? value =
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