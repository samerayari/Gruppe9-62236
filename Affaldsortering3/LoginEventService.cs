using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Affaldsortering3.Data;

namespace Affaldsortering3;

// Denne klasse bruges til at gemme og hente login-historik
// Den bruges til at vise hvem der har logget ind, og om det lykkedes
public class LoginEventService
{
    // Reference til databasen
    private readonly AppDbContext _db;

    // Constructor som får databasen sendt ind
    public LoginEventService(AppDbContext db)
    {
        _db = db;
    }

    // Gemmer et login-forsøg i databasen
    // Bruges både ved korrekt og forkert login
    public async Task AddAsync(string username, bool success, string message)
    {
        // Opretter et nyt login-event
        _db.LoginEvents.Add(new LoginEvent
        {
            // Gemmer brugernavnet (uden ekstra mellemrum)
            Username = (username ?? "").Trim(),

            // True hvis login lykkedes, false hvis det fejlede
            Success = success,

            // Tidspunkt for login-forsøget
            Time = DateTime.Now,

            // Ekstra besked (fx "Wrong password")
            Message = message ?? ""
        });

        // Gemmer ændringen i databasen
        await _db.SaveChangesAsync();
    }

    // Henter de nyeste login-events fra databasen
    // Bruges til at vise systemhistorik i GUI'en
    public Task<List<LoginEvent>> GetLatestAsync(int take = 200)
        => _db.LoginEvents
            // Sorterer så de nyeste kommer først
            .OrderByDescending(e => e.Time)
            // Begrænser antal (standard 200)
            .Take(take)
            // Laver det om til en liste
            .ToListAsync();
}