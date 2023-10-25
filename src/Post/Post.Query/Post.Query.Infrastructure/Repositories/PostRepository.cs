using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DataAccess;

namespace Post.Query.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly DatabaseContextFactory _contextFactory;

    public PostRepository(DatabaseContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task CreateAsync(PostEntity post)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }
        
    }

    public async Task UpdateAsync(PostEntity post)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            context.Posts.Update(post);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid postId)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            var post = await GetByIdAsync(postId);

            if (post is null) return;
            
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }
    }

    public async Task<PostEntity> GetByIdAsync(Guid postId)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            return await context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(x => x.PostId == postId) ?? throw new InvalidOperationException($"Post with id {postId} does not exist");
        }
    }

    public async Task<List<PostEntity>> ListAllAsync()
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            return await context.Posts.AsNoTracking() //No tracking for those entities, loads faster. Kinda read only
                .Include(p => p.Comments).AsNoTracking()
                .ToListAsync();
        }
    }

    public async Task<List<PostEntity>> ListByAuthorAsync(string author)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            return await context.Posts.AsNoTracking() //No tracking for those entities, loads faster. Kinda read only
                .Include(p => p.Comments).AsNoTracking()
                .Where(x =>x.Author.Contains(author))
                .ToListAsync();
        }
    }

    public async Task<List<PostEntity>> ListWithLikesAsync(int numberOfLikes)
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            return await context.Posts.AsNoTracking() //No tracking for those entities, loads faster. Kinda read only
                .Include(p => p.Comments).AsNoTracking()
                .Where(x => x.Likes >= numberOfLikes)
                .ToListAsync();
        }
    }

    public async Task<List<PostEntity>> ListWithCommentAsync()
    {
        using (CustomDatabaseContext context = _contextFactory.CreateDbContext())
        {
            return await context.Posts.AsNoTracking() //No tracking for those entities, loads faster. Kinda read only
                .Include(p => p.Comments).AsNoTracking()
                .Where(x => x.Comments.Any())
                .ToListAsync();
        }
    }
}