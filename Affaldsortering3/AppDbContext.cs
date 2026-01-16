using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

namespace Affaldsortering3;

public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginEvent> LoginEvents => Set<LoginEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        optionsBuilder.UseSqlite("Data Source=app.db");
    }
}