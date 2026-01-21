// Fil: Affaldsortering3/Data/LoginEvent.cs
// Hvad gør denne fil?
// - Den beskriver, hvordan et login-event ser ud i databasen.
// - Et login-event er en log over, når nogen prøver at logge ind.
// - Bruges til at gemme login-historik (fx i Database-fanen i GUI'en).

using System;

namespace Affaldsortering3.Data;

// Denne klasse repræsenterer ÉT login-forsøg
public class LoginEvent
{
    // Unikt nummer for login-eventet i databasen
    // Bruges kun internt af databasen
    public int Id { get; set; }

    // Brugernavnet der blev brugt ved login
    public string Username { get; set; } = "";

    // Viser om login lykkedes eller ej
    // true  = korrekt login
    // false = forkert login
    public bool Success { get; set; }

    // Tidspunkt for login-forsøget
    // Sættes automatisk til nuværende tidspunkt
    public DateTime Time { get; set; } = DateTime.Now;

    // Ekstra besked om login-forsøget
    // Fx "Wrong credentials" eller "Logged in"
    public string Message { get; set; } = "";
}