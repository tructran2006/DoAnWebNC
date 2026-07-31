using DoAn.Models;
using DoAn.Models.Enums;
using DoAn.Services;
using DoAn.ViewModels;
using DoAn.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoAn.Controllers;

public class AuthController : Controller
{
    private readonly UserService _userService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(UserService userService)
    {
        _userService = userService;
        _passwordHasher = new PasswordHasher<User>();
    }

    // =========================================================
    // LOGIN GET
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(UserRole.Admin.ToString()))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            return RedirectToAction(
                "Index",
                "Home");
        }

        ViewBag.ReturnUrl = returnUrl;

        return View(new LoginViewModel());
    }

    // =========================================================
    // LOGIN POST
    // =========================================================

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string account =
            model.UsernameOrEmail.Trim();

        var user = account.Contains('@')
            ? await _userService.GetByEmailAsync(
                account.ToLowerInvariant())
            : await _userService.GetByUsernameAsync(account);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tên đăng nhập, email hoặc mật khẩu không đúng.");

            return View(model);
        }

        bool passwordValid =
            VerifyPassword(user, model.Password);

        if (!passwordValid)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tên đăng nhập, email hoặc mật khẩu không đúng.");

            return View(model);
        }

        // =====================================================
        // KIỂM TRA TRẠNG THÁI
        // =====================================================

        if (user.Status == UserStatus.Suspended)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản của bạn đang bị tạm khóa. Vui lòng liên hệ quản trị viên.");

            return View(model);
        }

        if (user.Status == UserStatus.Banned)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản của bạn đã bị cấm truy cập.");

            return View(model);
        }

        if (user.Status != UserStatus.Active)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tài khoản hiện không thể đăng nhập.");

            return View(model);
        }

        // =====================================================
        // CHUYỂN MẬT KHẨU CŨ SANG HASH
        // =====================================================

        if (!IsHashedPassword(user.PasswordHash))
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    model.Password);
        }

        user.LastLoginAt = DateTime.UtcNow;

        await _userService.UpdateAsync(user);

        // =====================================================
        // TẠO CLAIMS
        // =====================================================

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                "FullName",
                user.FullName ?? string.Empty),

            new(
                "AvatarUrl",
                user.AvatarUrl ?? string.Empty)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        var authenticationProperties =
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true
            };

        if (model.RememberMe)
        {
            authenticationProperties.ExpiresUtc =
                DateTimeOffset.UtcNow.AddDays(7);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);

        // =====================================================
        // RETURN URL
        // =====================================================

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        // =====================================================
        // PHÂN QUYỀN
        // =====================================================

        if (user.Role == UserRole.Admin)
        {
            return RedirectToAction(
                "Dashboard",
                "Admin");
        }

        return RedirectToAction(
            "Index",
            "Home");
    }

    // =========================================================
    // REGISTER GET
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        return View(new RegisterViewModel());
    }

    // =========================================================
    // REGISTER POST
    // =========================================================

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string username =
            model.Username.Trim();

        string email =
            model.Email.Trim().ToLowerInvariant();

        string fullName =
            model.FullName.Trim();

        if (await _userService.ExistsByUsernameAsync(username))
        {
            ModelState.AddModelError(
                nameof(model.Username),
                "Tên đăng nhập đã tồn tại.");
        }

        if (await _userService.ExistsByEmailAsync(email))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Email đã được sử dụng.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Username = username,
            Email = email,
            FullName = fullName,
            Role = UserRole.User,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                model.Password);

        await _userService.CreateAsync(user);

        TempData["SuccessMessage"] =
            "Đăng ký thành công. Vui lòng đăng nhập.";

        return RedirectToAction(nameof(Login));
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["SuccessMessage"] =
            "Bạn đã đăng xuất thành công.";

        return RedirectToAction(nameof(Login));
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // =========================================================
    // PASSWORD HELPERS
    // =========================================================

    private bool VerifyPassword(
        User user,
        string inputPassword)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return false;
        }

        // Mật khẩu cũ đang lưu dạng text thường
        if (!IsHashedPassword(user.PasswordHash))
        {
            return string.Equals(
                user.PasswordHash,
                inputPassword,
                StringComparison.Ordinal);
        }

        try
        {
            var result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    inputPassword);

            return result is
                PasswordVerificationResult.Success or
                PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsHashedPassword(
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        // PasswordHasher của ASP.NET Core thường sinh chuỗi Base64
        // bắt đầu bằng AQAAAA...
        return passwordHash.StartsWith(
            "AQAAAA",
            StringComparison.Ordinal);
    }
}