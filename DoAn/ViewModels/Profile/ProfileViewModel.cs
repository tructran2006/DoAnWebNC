using DoAn.ViewModels.Post;

namespace DoAn.ViewModels.Profile;

public class ProfileViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FriendCount { get; set; }
    public int PostCount { get; set; }
    public bool IsOwnProfile { get; set; }
    public bool IsFriend { get; set; }
    public bool HasSentFriendRequest { get; set; }
    public bool HasReceivedFriendRequest { get; set; }
    public int? ReceivedRequestId { get; set; }
    public List<FeedPostViewModel> Posts { get; set; } = new();
}
