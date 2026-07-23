using DoAn.Data;
using DoAn.Models;
using DoAn.ViewModels.Post;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Services;

public class PostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .Include(x => x.MediaItems.OrderBy(m => m.DisplayOrder))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<Post?> GetForEditAsync(int id, int userId)
    {
        return await _context.Posts
            .Include(x => x.MediaItems.OrderBy(m => m.DisplayOrder))
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId &&
                !x.IsDeleted);
    }

    public async Task<List<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(x => x.MediaItems.OrderBy(m => m.DisplayOrder))
            .Where(x => !x.IsDeleted && !x.IsHidden)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Post>> GetUserPostsAsync(int userId)
    {
        return await _context.Posts
            .Include(x => x.MediaItems.OrderBy(m => m.DisplayOrder))
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id);
        if (post == null) return;

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<List<FeedPostViewModel>> GetFeedAsync(int currentUserId)
    {
        return await BuildFeedQuery(currentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<FeedPostViewModel>> GetProfilePostsAsync(
        int profileUserId,
        int currentUserId)
    {
        return await BuildFeedQuery(currentUserId)
            .Where(x => x.UserId == profileUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<FeedPostViewModel>> GetMyPostsAsync(int userId)
    {
        return await GetProfilePostsAsync(userId, userId);
    }

    private IQueryable<FeedPostViewModel> BuildFeedQuery(int currentUserId)
    {
        return _context.Posts
            .AsNoTracking()
            .Where(post => !post.IsDeleted && !post.IsHidden)
            .Select(post => new FeedPostViewModel
            {
                Id = post.Id,
                UserId = post.UserId,
                FullName = post.User != null ? post.User.FullName : "Người dùng",
                AvatarUrl = post.User != null ? post.User.AvatarUrl : null,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                LikeCount = _context.Likes.Count(like => like.PostId == post.Id),
                CommentCount = _context.Comments.Count(comment =>
                    comment.PostId == post.Id && !comment.IsDeleted),
                IsLikedByCurrentUser = currentUserId > 0 && _context.Likes.Any(like =>
                    like.PostId == post.Id && like.UserId == currentUserId),
                IsOwner = currentUserId > 0 && post.UserId == currentUserId,
                MediaItems = post.MediaItems
                    .OrderBy(media => media.DisplayOrder)
                    .Select(media => new PostMediaViewModel
                    {
                        Id = media.Id,
                        MediaUrl = media.MediaUrl,
                        MediaType = media.MediaType,
                        DisplayOrder = media.DisplayOrder
                    })
                    .ToList()
            });
    }

    public async Task<PostDetailsViewModel?> GetDetailsAsync(int id, int currentUserId)
    {
        var feedPost = await BuildFeedQuery(currentUserId)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (feedPost == null) return null;

        var post = new PostDetailsViewModel
        {
            Id = feedPost.Id,
            UserId = feedPost.UserId,
            FullName = feedPost.FullName,
            AvatarUrl = feedPost.AvatarUrl,
            Content = feedPost.Content,
            CreatedAt = feedPost.CreatedAt,
            UpdatedAt = feedPost.UpdatedAt,
            LikeCount = feedPost.LikeCount,
            IsLikedByCurrentUser = feedPost.IsLikedByCurrentUser,
            IsOwner = feedPost.IsOwner,
            MediaItems = feedPost.MediaItems
        };

        post.Comments = await (
            from comment in _context.Comments.AsNoTracking()
            join user in _context.Users.AsNoTracking()
                on comment.UserId equals user.Id
            where comment.PostId == id && !comment.IsDeleted
            orderby comment.CreatedAt
            select new CommentItemViewModel
            {
                Id = comment.Id,
                UserId = comment.UserId,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            }).ToListAsync();

        return post;
    }

    public async Task ToggleLikeAsync(int postId, int userId)
    {
        var existing = await _context.Likes.FirstOrDefaultAsync(x =>
            x.PostId == postId && x.UserId == userId);

        if (existing == null)
        {
            _context.Likes.Add(new Like
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            _context.Likes.Remove(existing);
        }

        await _context.SaveChangesAsync();
    }

    public Task<int> GetLikeCountAsync(int postId) =>
        _context.Likes.CountAsync(x => x.PostId == postId);

    public Task<bool> IsLikedAsync(int postId, int userId) =>
        _context.Likes.AnyAsync(x => x.PostId == postId && x.UserId == userId);

    public async Task<List<FeedPostViewModel>> SearchPostsAsync(
        string keyword,
        int currentUserId,
        int take = 30)
    {
        keyword = keyword.Trim();
        var ids = await _context.Posts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsHidden &&
                EF.Functions.Like(x.Content, $"%{keyword}%"))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Id)
            .Take(take)
            .ToListAsync();

        return await BuildFeedQuery(currentUserId)
            .Where(x => ids.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
