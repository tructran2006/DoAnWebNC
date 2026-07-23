using DoAn.Data;
using DoAn.Models;
using Microsoft.EntityFrameworkCore;


namespace DoAn.Services;

public class CommentService
{
    private readonly AppDbContext _context;

    public CommentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Comment>>
        GetPostCommentsAsync(int postId)
    {
        return await _context.Comments
            .Where(x =>
                x.PostId == postId &&
                !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var comment =
            await _context.Comments.FindAsync(id);

        if (comment == null)
            return;

        comment.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}