using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.Post;

public class EditPostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Thêm ảnh hoặc video")]
    public List<IFormFile> MediaFiles { get; set; } = new();

    public List<PostMediaViewModel> CurrentMedia { get; set; } = new();

    public List<int> RemoveMediaIds { get; set; } = new();
}
