namespace DoAn.ViewModels.Post;

public class PostMediaViewModel
{
    public int Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image";
    public int DisplayOrder { get; set; }
}
