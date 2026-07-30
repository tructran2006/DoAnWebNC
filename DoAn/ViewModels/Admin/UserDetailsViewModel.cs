using DoAn.Models.Enums;

namespace DoAn.ViewModels.Admin;

/// <summary>
/// Dữ liệu dùng riêng cho màn hình Admin xem chi tiết người dùng.
/// Không truyền trực tiếp Entity User sang View để tránh View phải tự truy vấn thêm dữ liệu.
/// </summary>
public class UserDetailsViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public string? Bio { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Các số liệu tổng hợp để hiển thị ở trang chi tiết.
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    public int TotalFriends { get; set; }
}
