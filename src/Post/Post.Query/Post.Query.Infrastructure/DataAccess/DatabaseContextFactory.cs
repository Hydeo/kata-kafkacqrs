using Microsoft.EntityFrameworkCore;

namespace Post.Query.Infrastructure.DataAccess;

public class DatabaseContextFactory
{
    private readonly Action<DbContextOptionsBuilder> _configureDbContext;

    public DatabaseContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
    {
        _configureDbContext = configureDbContext;
    }

    public CustomDatabaseContext CreateDbContext()
    {
        DbContextOptionsBuilder<CustomDatabaseContext> options = new();
        _configureDbContext(options);
        return new CustomDatabaseContext(options.Options);
    }
}