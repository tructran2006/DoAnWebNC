using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
    public string UsernameOrEmail { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}