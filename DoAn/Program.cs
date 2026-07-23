using DoAn.Data;
using DoAn.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// DATABASE
// =========================================================

string? connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Không tìm thấy ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// =========================================================
// SERVICES
// =========================================================

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<FriendService>();
builder.Services.AddScoped<NotificationService>();

// =========================================================
// AUTHENTICATION
// =========================================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";

        options.ExpireTimeSpan =
            TimeSpan.FromDays(7);

        options.SlidingExpiration = true;

        /*
         * Với request AJAX, tránh chuyển hướng HTML về trang đăng nhập.
         * Trả 401/403 để JavaScript xử lý đúng.
         */
        options.Events.OnRedirectToLogin = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            }

            context.Response.Redirect(
                context.RedirectUri);

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (IsAjaxRequest(context.Request))
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                return Task.CompletedTask;
            }

            context.Response.Redirect(
                context.RedirectUri);

            return Task.CompletedTask;
        };
    });

// =========================================================
// MVC
// =========================================================

builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================================================
// AUTO MIGRATION
// =========================================================

using (IServiceScope scope =
       app.Services.CreateScope())
{
    AppDbContext db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    try
    {
        db.Database.Migrate();
    }
    catch (Exception exception)
    {
        ILogger<Program> logger =
            scope.ServiceProvider
                .GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exception,
            "Không thể cập nhật database.");

        throw;
    }
}

// =========================================================
// ERROR HANDLING
// =========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// =========================================================
// MIDDLEWARE
// =========================================================

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// ROUTING
// =========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

static bool IsAjaxRequest(
    HttpRequest request)
{
    return string.Equals(
               request.Headers["X-Requested-With"],
               "XMLHttpRequest",
               StringComparison.OrdinalIgnoreCase)
           ||
           request.Headers.Accept.Any(value =>
               value?.Contains(
                   "application/json",
                   StringComparison.OrdinalIgnoreCase)
               == true);
}