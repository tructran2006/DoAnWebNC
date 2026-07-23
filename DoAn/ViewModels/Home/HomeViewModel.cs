using DoAn.Models;
using DoAn.ViewModels.Home;
using DoAn.ViewModels.Post;

namespace DoAn.ViewModels;

public class HomeViewModel
{
    public List<FeedPostViewModel> Posts { get; set; } = new();

    public List<User> SuggestedUsers { get; set; } = new();

    public List<FeaturedPostViewModel> TopPosts { get; set; } = new();
}