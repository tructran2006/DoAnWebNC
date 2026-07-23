using DoAn.Data;
using DoAn.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Services;

public class FriendService
{
    private readonly AppDbContext _context;

    public FriendService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FriendRequest?> GetRequestByIdAsync(int requestId)
    {
        return await _context.FriendRequests
            .FirstOrDefaultAsync(x => x.Id == requestId);
    }

    public async Task SendRequestAsync(
        int senderId,
        int receiverId)
    {
        if (senderId == receiverId || await AreFriendsAsync(senderId, receiverId))
            return;

        var reverseExists = await _context.FriendRequests
            .AnyAsync(x =>
                x.SenderId == receiverId &&
                x.ReceiverId == senderId &&
                !x.IsAccepted &&
                !x.IsRejected);

        if (reverseExists)
            return;
        var exists = await _context.FriendRequests
            .AnyAsync(x =>
                x.SenderId == senderId &&
                x.ReceiverId == receiverId &&
                !x.IsAccepted &&
                !x.IsRejected);

        if (exists)
            return;

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId
        };

        _context.FriendRequests.Add(request);

        await _context.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(int requestId)
    {
        var request =
            await _context.FriendRequests.FindAsync(requestId);

        if (request == null)
            return;

        request.IsAccepted = true;

        _context.Friends.Add(new Friend
        {
            UserId = request.SenderId,
            FriendId = request.ReceiverId
        });

        _context.Friends.Add(new Friend
        {
            UserId = request.ReceiverId,
            FriendId = request.SenderId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(int requestId)
    {
        var request =
            await _context.FriendRequests.FindAsync(requestId);

        if (request == null)
            return;

        request.IsRejected = true;

        await _context.SaveChangesAsync();
    }

    public async Task<List<FriendRequest>>
        GetPendingRequestsAsync(int userId)
    {
        return await _context.FriendRequests
            .Where(x =>
                x.ReceiverId == userId &&
                !x.IsAccepted &&
                !x.IsRejected)
            .ToListAsync();
    }

    public async Task<List<Friend>>
        GetFriendsAsync(int userId)
    {
        return await _context.Friends
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool>
        AreFriendsAsync(int userId, int friendId)
    {
        return await _context.Friends
            .AnyAsync(x =>
                x.UserId == userId &&
                x.FriendId == friendId);
    }

    public async Task<FriendRequest?> GetPendingRequestAsync(int senderId, int receiverId)
    {
        return await _context.FriendRequests
            .FirstOrDefaultAsync(x =>
                x.SenderId == senderId &&
                x.ReceiverId == receiverId &&
                !x.IsAccepted &&
                !x.IsRejected);
    }

    public async Task RemoveFriendAsync(
        int userId,
        int friendId)
    {
        var friends = await _context.Friends
            .Where(x =>
                (x.UserId == userId &&
                 x.FriendId == friendId)
                 ||
                (x.UserId == friendId &&
                 x.FriendId == userId))
            .ToListAsync();

        _context.Friends.RemoveRange(friends);

        await _context.SaveChangesAsync();
    }
}