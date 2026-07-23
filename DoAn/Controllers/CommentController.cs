using System.Security.Claims;
using DoAn.Models;
using DoAn.Services;
using DoAn.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers;

[Authorize]
public class CommentController : Controller
{
    private readonly CommentService _commentService;
    private readonly PostService _postService;
    private readonly NotificationService _notificationService;

    public CommentController(
        CommentService commentService,
        PostService postService,
        NotificationService notificationService)
    {
        _commentService = commentService;
        _postService = postService;
        _notificationService = notificationService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCommentViewModel model)
    {
        var post = await _postService.GetByIdAsync(model.PostId);

        if (post == null || post.IsDeleted || post.IsHidden)
        {
            return NotFound();
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Content))
        {
            TempData["CommentError"] =
                "Vui lòng nhập nội dung bình luận.";

            return RedirectToPostComments(model.PostId);
        }

        var comment = new Comment
        {
            PostId = model.PostId,
            UserId = GetCurrentUserId(),
            Content = model.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _commentService.CreateAsync(comment);

        if (post.UserId != comment.UserId)
        {
            await _notificationService.CreateOrRefreshAsync(
                receiverId: post.UserId,
                actorUserId: comment.UserId,
                type: "Comment",
                referenceId: post.Id,
                message: "Có người đã bình luận bài viết của bạn.",
                url: $"/Post/Details/{post.Id}#comments");
        }

        TempData["CommentSuccess"] =
            "Đã gửi bình luận.";

        return RedirectToPostComments(model.PostId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int postId)
    {
        var comment = await _commentService.GetByIdAsync(id);

        if (comment == null || comment.IsDeleted)
        {
            return NotFound();
        }

        if (comment.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        await _commentService.DeleteAsync(id);

        TempData["CommentSuccess"] =
            "Đã xóa bình luận.";

        return RedirectToPostComments(postId);
    }

    private IActionResult RedirectToPostComments(int postId)
    {
        return Redirect($"/Post/Details/{postId}#comments");
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId))
        {
            throw new InvalidOperationException(
                "Không xác định được người dùng đang đăng nhập.");
        }

        return userId;
    }
}
