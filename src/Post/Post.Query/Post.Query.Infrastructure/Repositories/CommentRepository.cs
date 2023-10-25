using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly DatabaseContextFactory _contextFactory;
    
    
    public CommentRepository(DatabaseContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task CreateAsync(CommentEntity comment)
    {
        using var customDatabaseContext = _contextFactory.CreateDbContext();
        customDatabaseContext.Add(comment);
        await customDatabaseContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(CommentEntity comment)
    {
        using var customDatabaseContext = _contextFactory.CreateDbContext();
        customDatabaseContext.Update(comment);
        await customDatabaseContext.SaveChangesAsync();
    }

    public async Task<CommentEntity> GetByIdAsync(Guid commentId)
    {
        using var customDatabaseContext = _contextFactory.CreateDbContext();
        return await customDatabaseContext.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId)
               ?? throw new InvalidOperationException($"Comment with id {commentId} does not exist");;
    }

    public async Task DeleteAsync(Guid commentId)
    {
        using var customDatabaseContext = _contextFactory.CreateDbContext();
        var comment = await GetByIdAsync(commentId);
        
        if (comment is null) return;

        customDatabaseContext.Remove(comment);
        await customDatabaseContext.SaveChangesAsync();
    }
}