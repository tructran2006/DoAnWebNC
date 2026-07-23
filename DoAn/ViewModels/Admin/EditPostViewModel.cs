using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.Admin;

public class EditPostViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
    [StringLength(
        5000,
        ErrorMessage = "Nội dung không được vượt quá 5000 ký tự.")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Ẩn bài viết")]
    public bool IsHidden { get; set; }

    [Display(Name = "Đánh dấu đã xóa")]
    public bool IsDeleted { get; set; }
}