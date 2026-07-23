namespace DoAn.ViewModels.Post;

public class PostDetailsViewModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsOwner { get; set; }
    public List<PostMediaViewModel> MediaItems { get; set; } = new();
    public List<CommentItemViewModel> Comments { get; set; } = new();
}
