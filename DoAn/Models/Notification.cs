namespace DoAn.Models;

public class Notification
{
    public int Id { get; set; }

    // Người nhận thông báo
    public int UserId { get; set; }

    // Người tạo ra tương tác
    public int? ActorUserId { get; set; }

    // Like, Comment, FriendRequest, FriendAccepted
    public string Type { get; set; } = string.Empty;

    // ID bài viết hoặc lời mời liên quan
    public int? ReferenceId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Url { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? ActorUser { get; set; }
}