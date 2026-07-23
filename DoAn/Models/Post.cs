using DoAn.Models.Enums;

namespace DoAn.Models;

public class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? MediaUrl { get; set; }

    public string? MediaType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsHidden { get; set; }

    public PostVisibility Visibility { get; set; }

    public ICollection<PostMedia> MediaItems { get; set; } = new List<PostMedia>();
}