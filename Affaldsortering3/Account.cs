using System;

namespace Affaldsortering3.Data
{
    // Denne klasse beskriver en bruger i systemet
    public class Account
    {
        // Brugerens unikke nummer i databasen
        public int Id { get; set; }

        // Det navn brugeren logger ind med
        public string Username { get; set; } = "";

        // Bruges til at gøre password mere sikkert
        public byte[] Salt { get; set; } = Array.Empty<byte>();

        // Det sikrede (hashede) password
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        // Viser om brugeren er administrator eller almindelig bruger
        public bool IsAdmin { get; set; }
    }
}