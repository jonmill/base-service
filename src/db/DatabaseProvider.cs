using Microsoft.EntityFrameworkCore;

namespace Warbreaker.Data;

public sealed class DatabaseProvider : DbContext
{
    public DatabaseProvider(DbContextOptions<DatabaseProvider> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {

    }
}
