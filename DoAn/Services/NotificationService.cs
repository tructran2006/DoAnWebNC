using DoAn.Data;
using DoAn.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Notification notification)
    {
        // Không tự gửi thông báo cho chính mình
        if (notification.ActorUserId.HasValue &&
            notification.ActorUserId.Value == notification.UserId)
        {
            return;
        }

        notification.CreatedAt = DateTime.UtcNow;
        notification.IsRead = false;

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task CreateOrRefreshAsync(
        int receiverId,
        int actorUserId,
        string type,
        int? referenceId,
        string message,
        string? url)
    {
        if (receiverId == actorUserId)
        {
            return;
        }

        // Tránh tạo nhiều thông báo giống nhau khi người dùng
        // bỏ thích rồi thích lại liên tục.
        var existing = await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.UserId == receiverId &&
                x.ActorUserId == actorUserId &&
                x.Type == type &&
                x.ReferenceId == referenceId &&
                !x.IsRead);

        if (existing != null)
        {
            existing.Message = message;
            existing.Url = url;
            existing.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return;
        }

        var notification = new Notification
        {
            UserId = receiverId,
            ActorUserId = actorUserId,
            Type = type,
            ReferenceId = referenceId,
            Message = message,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(
        int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(x =>
                x.UserId == userId &&
                !x.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(
        int notificationId,
        int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.Id == notificationId &&
                x.UserId == userId);

        if (notification == null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(x =>
                x.UserId == userId &&
                !x.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        int notificationId,
        int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.Id == notificationId &&
                x.UserId == userId);

        if (notification == null)
        {
            return;
        }

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return;
        }

        _context.Notifications.RemoveRange(notifications);

        await _context.SaveChangesAsync();
    }
}