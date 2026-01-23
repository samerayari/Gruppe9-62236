using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

// Denne klasse bruges til alt med brugere og login
// GUI’en bruger denne klasse, når man logger ind eller opretter brugere
namespace Affaldsortering3;

public class AccountService
{
    // Forbindelse til databasen
    private readonly AppDbContext _db;

    // Bruges til at sikre passwords (hash og tjek)
    private readonly PasswordHasher _hasher;

    // Starter AccountService og giver adgang til database og password-sikkerhed
    public AccountService(AppDbContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // Sørger for at databasen findes og at der altid er en admin-bruger
    public async Task EnsureDbAsync()
    {
        // Opretter databasen hvis den ikke allerede findes
        await _db.Database.EnsureCreatedAsync();

        // Tjekker om der allerede findes en admin-bruger
        var exists = await _db.Accounts.AnyAsync(a => a.Username == "admin");
        if (!exists)
        {
            // Opretter en standard admin (admin / admin)
            await CreateUserAsync("admin", "admin", true);
        }
    }

    // Opretter en ny bruger i databasen
    public async Task CreateUserAsync(string username, string password, bool isAdmin)
    {
        // Stopper hvis brugernavn eller password er tomt
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new Exception("Username/password må ikke være tom.");

        // Fjerner ekstra mellemrum i brugernavnet
        username = username.Trim();

        // Tjekker om brugeren allerede findes
        var exists = await _db.Accounts.AnyAsync(a => a.Username == username);
        if (exists) throw new Exception("Bruger findes allerede.");

        // Laver password om til en sikker version (hash + salt)
        var (salt, hash) = _hasher.Hash(password);

        // Gemmer den nye bruger i databasen
        _db.Accounts.Add(new Account
        {
            Username = username,
            Salt = salt,
            PasswordHash = hash,
            IsAdmin = isAdmin
        });

        // Gemmer ændringerne permanent
        await _db.SaveChangesAsync();
    }

    // Tjekker om login-oplysninger er korrekte
    public async Task<(bool ok, bool isAdmin)> ValidateAsync(string username, string password)
    {
        // Sikrer at username ikke er null og fjerner mellemrum
        username = (username ?? "").Trim();

        // Finder brugeren i databasen
        var acc = await _db.Accounts.FirstOrDefaultAsync(a => a.Username == username);
        if (acc == null) return (false, false);

        // Tjekker om password matcher det gemte password
        var ok = _hasher.Verify(password ?? "", acc.Salt, acc.PasswordHash);

        // Returnerer om login er korrekt og om brugeren er admin
        return (ok, ok && acc.IsAdmin);
    }

    // Henter alle brugere fra databasen
    public Task<List<Account>> GetUsersAsync()
        => _db.Accounts.OrderBy(a => a.Username).ToListAsync();

    // Nulstiller databasen og opretter den igen
    public async Task RecreateDbAsync()
    {
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();
        await EnsureDbAsync();
    }
}