namespace DoAn.Models;

public class FriendRequest
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsAccepted { get; set; }

    public bool IsRejected { get; set; }
}