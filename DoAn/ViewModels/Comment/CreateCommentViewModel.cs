using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.Post;

public class CreateCommentViewModel
{
    public int PostId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận.")]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;
}