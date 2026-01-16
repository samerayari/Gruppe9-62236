using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

namespace Affaldsortering3;

public class AccountService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;

    public AccountService(AppDbContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task EnsureDbAsync()
    {
        await _db.Database.EnsureCreatedAsync();

        // Default admin (admin/admin)
        var exists = await _db.Accounts.AnyAsync(a => a.Username == "admin");
        if (!exists)
        {
            await CreateUserAsync("admin", "admin", true);
        }
    }

    public async Task CreateUserAsync(string username, string password, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new Exception("Username/password må ikke være tom.");

        username = username.Trim();

        var exists = await _db.Accounts.AnyAsync(a => a.Username == username);
        if (exists) throw new Exception("Bruger findes allerede.");

        var (salt, hash) = _hasher.Hash(password);

        _db.Accounts.Add(new Account
        {
            Username = username,
            Salt = salt,
            PasswordHash = hash,
            IsAdmin = isAdmin
        });

        await _db.SaveChangesAsync();
    }

    public async Task<(bool ok, bool isAdmin)> ValidateAsync(string username, string password)
    {
        username = (username ?? "").Trim();

        var acc = await _db.Accounts.FirstOrDefaultAsync(a => a.Username == username);
        if (acc == null) return (false, false);

        var ok = _hasher.Verify(password ?? "", acc.Salt, acc.PasswordHash);
        return (ok, ok && acc.IsAdmin);
    }

    public Task<List<Account>> GetUsersAsync()
        => _db.Accounts.OrderBy(a => a.Username).ToListAsync();

    public async Task RecreateDbAsync()
    {
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();
        await EnsureDbAsync();
    }
}
