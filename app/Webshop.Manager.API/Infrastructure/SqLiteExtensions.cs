using Microsoft.EntityFrameworkCore;

namespace Webshop.Manager.API.Infrastructure;

public static class SqLiteExtensions
{
    public static async Task AddWalMode(this WebApplication app)
    {
        // SQLite WAL mode + busy timeout (prevents "database is locked" under concurrent access)
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        foreach (var db in new DbContext[]
                 {
                     sp.GetRequiredService<Data.WebshopDbContext>(),
                     sp.GetRequiredService<Webshop.Identity.Data.AccountsDbContext>()
                 })
        {
            await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
            await db.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout=5000;");
        }
    }   
}