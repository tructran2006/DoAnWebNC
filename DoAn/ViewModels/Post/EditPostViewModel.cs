using DoAn.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.Post;

public class EditPostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
    [StringLength(
        5000,
        ErrorMessage = "Nội dung không được vượt quá 5000 ký tự.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn quyền riêng tư.")]
    [Display(Name = "Quyền riêng tư")]
    public PostVisibility Visibility { get; set; }

    [Display(Name = "Thêm ảnh hoặc video")]
    public List<IFormFile> MediaFiles { get; set; } = new();

    public List<PostMediaViewModel> CurrentMedia { get; set; } = new();

    public List<int> RemoveMediaIds { get; set; } = new();
}