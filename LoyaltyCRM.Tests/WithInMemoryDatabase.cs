using LoyaltyCRM.Infrastructure.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class WithInMemoryDatabase : IDisposable
{
    public (LoyaltyContext context, SqliteConnection connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<LoyaltyContext>()
            .UseSqlite(connection)
            .Options;

        var context = new LoyaltyContext(options);

        // Ensure schema is created
        context.Database.EnsureCreated();

        return (context, connection);
    }
    public virtual void Dispose()
    {
        
    }
}

