using System.ComponentModel.DataAnnotations;
using DoAn.Models.Enums;

namespace DoAn.ViewModels.Admin;

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(50)]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Ảnh bìa")]
    public string? CoverUrl { get; set; }

    [StringLength(500)]
    [Display(Name = "Giới thiệu")]
    public string? Bio { get; set; }

    [Display(Name = "Vai trò")]
    public UserRole Role { get; set; }

    [Display(Name = "Trạng thái")]
    public UserStatus Status { get; set; }
}