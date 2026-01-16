using System;

namespace Affaldsortering3.Data;

public class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public byte[] Salt { get; set; } = Array.Empty<byte>();

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public bool IsAdmin { get; set; }
}