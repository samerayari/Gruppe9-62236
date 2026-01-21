using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

namespace Affaldsortering3;

// Denne klasse styrer forbindelsen til databasen
// Den fortæller programmet hvilke tabeller databasen har
public class AppDbContext : DbContext
{
    // Tabel til brugerkonti (login, roller osv.)
    public DbSet<Account> Accounts => Set<Account>();

    // Tabel til login-historik (hvem loggede ind, hvornår osv.)
    public DbSet<LoginEvent> LoginEvents => Set<LoginEvent>();

    // Her opsættes hvilken database der bruges
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Bruger en lokal SQLite-database med navnet app.db
        optionsBuilder.UseSqlite("Data Source=app.db");
    }
}