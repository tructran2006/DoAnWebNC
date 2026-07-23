namespace DoAn.ViewModels.Profile;

public class UserCardViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Bio { get; set; } = string.Empty;
    public bool IsFriend { get; set; }
}
