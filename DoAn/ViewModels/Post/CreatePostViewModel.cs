using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.Post;

public class CreatePostViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Ảnh hoặc video")]
    public List<IFormFile> MediaFiles { get; set; } = new();
}
