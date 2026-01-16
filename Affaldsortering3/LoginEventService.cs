using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

namespace Affaldsortering3;

public class LoginEventService
{
    private readonly AppDbContext _db;

    public LoginEventService(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(string username, bool success, string message)
    {
        _db.LoginEvents.Add(new LoginEvent
        {
            Username = (username ?? "").Trim(),
            Success = success,
            Time = DateTime.Now,
            Message = message ?? ""
        });

        await _db.SaveChangesAsync();
    }

    public Task<List<LoginEvent>> GetLatestAsync(int take = 200)
        => _db.LoginEvents
            .OrderByDescending(e => e.Time)
            .Take(take)
            .ToListAsync();
}