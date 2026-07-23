namespace DoAn.Models;

public class PostMedia
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image";
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Post? Post { get; set; }
}
