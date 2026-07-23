using DoAn.Models.Enums;

namespace DoAn.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public ICollection<Post> Posts { get; set; }
    = new List<Post>();

    public ICollection<Comment> Comments { get; set; }
        = new List<Comment>();

    public ICollection<Like> Likes { get; set; }
        = new List<Like>();
}