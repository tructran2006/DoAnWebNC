using DoAn.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // USER
        // =====================================================

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        // =====================================================
        // POST
        // =====================================================

        modelBuilder.Entity<Post>()
            .HasOne(post => post.User)
            .WithMany(user => user.Posts)
            .HasForeignKey(post => post.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // POST MEDIA
        // =====================================================

        modelBuilder.Entity<PostMedia>()
            .HasOne(media => media.Post)
            .WithMany(post => post.MediaItems)
            .HasForeignKey(media => media.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostMedia>()
            .Property(media => media.MediaUrl)
            .IsRequired();

        modelBuilder.Entity<PostMedia>()
            .Property(media => media.MediaType)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<PostMedia>()
            .HasIndex(media => new
            {
                media.PostId,
                media.DisplayOrder
            });

        // =====================================================
        // COMMENT
        // =====================================================

        modelBuilder.Entity<Comment>()
            .HasOne(comment => comment.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(comment => comment.Post)
            .WithMany()
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // LIKE
        // =====================================================

        modelBuilder.Entity<Like>()
            .HasOne(like => like.User)
            .WithMany(user => user.Likes)
            .HasForeignKey(like => like.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Like>()
            .HasOne(like => like.Post)
            .WithMany()
            .HasForeignKey(like => like.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasIndex(like => new
            {
                like.PostId,
                like.UserId
            })
            .IsUnique();

        // =====================================================
        // FRIEND
        // =====================================================

        modelBuilder.Entity<Friend>()
            .HasIndex(friend => new
            {
                friend.UserId,
                friend.FriendId
            })
            .IsUnique();

        // =====================================================
        // FRIEND REQUEST
        // =====================================================

        modelBuilder.Entity<FriendRequest>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FriendRequest>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);

        // =====================================================
        // NOTIFICATION
        // =====================================================

        modelBuilder.Entity<Notification>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.ActorUser)
            .WithMany()
            .HasForeignKey(notification => notification.ActorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Notification>()
            .HasIndex(notification => new
            {
                notification.UserId,
                notification.IsRead,
                notification.CreatedAt
            });
    }
}