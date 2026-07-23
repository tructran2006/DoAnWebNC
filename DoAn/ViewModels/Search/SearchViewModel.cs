using DoAn.ViewModels.Post;

namespace DoAn.ViewModels.Search;

public class SearchViewModel
{
    public string Query { get; set; } = string.Empty;

    public List<SearchUserViewModel> Users { get; set; }
        = new();

    public List<FeedPostViewModel> Posts { get; set; }
        = new();

    public int TotalResults =>
        Users.Count + Posts.Count;
}

public class SearchUserViewModel
{
    public int Id { get; set; }

    public string Username { get; set; }
        = string.Empty;

    public string FullName { get; set; }
        = string.Empty;

    public string Email { get; set; }
        = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }
}