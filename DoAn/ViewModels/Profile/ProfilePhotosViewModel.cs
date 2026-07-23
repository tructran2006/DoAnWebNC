namespace DoAn.ViewModels.Profile;

public class ProfilePhotoItemViewModel
{
    public int PostId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProfilePhotosViewModel
{
    public int ProfileId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
    public List<ProfilePhotoItemViewModel> Photos { get; set; } = [];
}
