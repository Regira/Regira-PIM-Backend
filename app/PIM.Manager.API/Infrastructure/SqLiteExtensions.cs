using Microsoft.EntityFrameworkCore;
using PIM.Identity.Data;

namespace PIM.Admin.API.Infrastructure;

public static class SqLiteExtensions
{
    public static async Task AddWalMode(this WebApplication app)
    {
        // SQLite WAL mode + busy timeout (prevents "database is locked" under concurrent access)
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        foreach (var db in new DbContext[]
                 {
                     sp.GetRequiredService<Data.PimDbContext>(),
                     sp.GetRequiredService<AccountsDbContext>()
                 })
        {
            if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
                await db.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;");
            }
        }
    }
}