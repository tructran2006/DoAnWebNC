namespace DoAn.ViewModels.Post;

public class FeedViewModel
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CommentCount { get; set; }
}