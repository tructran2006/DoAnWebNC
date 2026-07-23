using DoAn.Models.Enums;

namespace DoAn.ViewModels.Admin;

public class UserDetailsViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public string? Bio { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int TotalPosts { get; set; }

    public int TotalFriends { get; set; }
}