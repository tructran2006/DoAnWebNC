using DoAn.Data;
using DoAn.Models;
using DoAn.Models.Enums;
using DoAn.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User?> LoginAsync(
    string usernameOrEmail,
    string password)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x =>
                (x.Username == usernameOrEmail ||
                 x.Email == usernameOrEmail)
                 &&
                 x.PasswordHash == password);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Users
            .Where(x => idList.Contains(x.Id))
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;

        await _context.Users.AddAsync(user);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return;

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    public async Task LockUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return;

        user.Status = UserStatus.Suspended;

        await _context.SaveChangesAsync();
    }

    public async Task UnlockUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return;

        user.Status = UserStatus.Active;

        await _context.SaveChangesAsync();
    }

    public async Task MakeAdminAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return;

        user.Role = UserRole.Admin;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveAdminAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return;

        user.Role = UserRole.User;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users
            .AnyAsync(x => x.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        string value = email.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(x => x.Email.ToLower() == value);
    }

    public async Task<bool> ExistsByEmailExceptAsync(string email, int excludedUserId)
    {
        string value = email.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(x =>
                x.Id != excludedUserId &&
                x.Email.ToLower() == value);
    }

    /// <summary>
    /// Lấy đầy đủ dữ liệu của một người dùng để Admin xem chi tiết.
    /// Hàm này đọc dữ liệu từ nhiều bảng và gom lại thành UserDetailsViewModel.
    /// AsNoTracking được dùng vì đây chỉ là thao tác xem, không chỉnh sửa Entity.
    /// </summary>
    public async Task<UserDetailsViewModel?> GetDetailsAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return null;
        }

        return new UserDetailsViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            CoverUrl = user.CoverUrl,
            Bio = user.Bio,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,

            // Đếm dữ liệu liên quan trực tiếp trong database.
            TotalPosts = await _context.Posts.CountAsync(x =>
                x.UserId == userId && !x.IsDeleted),

            TotalComments = await _context.Comments.CountAsync(x =>
                x.UserId == userId && !x.IsDeleted),

            TotalLikes = await _context.Likes.CountAsync(x =>
                x.UserId == userId),

            // Bảng Friends lưu quan hệ hai chiều, nên chỉ cần đếm UserId.
            TotalFriends = await _context.Friends.CountAsync(x =>
                x.UserId == userId)
        };
    }
}