using System;

namespace Affaldsortering3.Data;

public class LoginEvent
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public bool Success { get; set; }
    public DateTime Time { get; set; } = DateTime.Now;
    public string Message { get; set; } = "";
}