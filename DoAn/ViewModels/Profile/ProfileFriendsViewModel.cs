namespace DoAn.ViewModels.Profile;

public class ProfileFriendsViewModel
{
    public int ProfileId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public bool IsOwnProfile { get; set; }
    public List<UserCardViewModel> Friends { get; set; } = [];
}
