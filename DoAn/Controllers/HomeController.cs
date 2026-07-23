using System.Security.Claims;
using DoAn.Data;
using DoAn.Models.Enums;
using DoAn.Services;
using DoAn.ViewModels;
using DoAn.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly PostService _postService;

    public HomeController(
        AppDbContext context,
        PostService postService)
    {
        _context = context;
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int currentUserId = GetCurrentUserId();

        var posts = await _postService.GetFeedAsync(
            currentUserId);

        var suggestedUsers = await _context.Users
            .AsNoTracking()
            .Where(user =>
                user.Status == UserStatus.Active &&
                user.Id != currentUserId)
            .OrderByDescending(user => user.CreatedAt)
            .Take(4)
            .ToListAsync();

        var topPosts = await _context.Posts
            .AsNoTracking()
            .Where(post =>
                !post.IsDeleted &&
                !post.IsHidden)
            .Select(post => new FeaturedPostViewModel
            {
                Id = post.Id,

                Content = post.Content,

                LikeCount = _context.Likes.Count(
                    like => like.PostId == post.Id)
            })
            .OrderByDescending(post => post.LikeCount)
            .ThenByDescending(post => post.Id)
            .Take(4)
            .ToListAsync();

        var model = new HomeViewModel
        {
            Posts = posts,
            SuggestedUsers = suggestedUsers,
            TopPosts = topPosts
        };

        return View(model);
    }

    private int GetCurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return 0;
        }

        string? value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(value, out int userId)
            ? userId
            : 0;
    }
}