using DoAn.Models;

namespace DoAn.ViewModels;

public class FriendPageViewModel
{
    public List<FriendRequest> Requests { get; set; }
        = new();

    public List<User> SuggestedUsers { get; set; }
        = new();

    public List<User> Friends { get; set; }
        = new();
}