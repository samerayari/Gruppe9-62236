using Avalonia;
using System;

namespace Affaldsortering3;

class Program
{
    // Dette er STARTPUNKTET for hele programmet
    // Det er her programmet begynder, når du trykker "Run" i Rider
    
    // [STAThread] er et krav for desktop-programmer (GUI)
    // Det sikrer, at vinduer og input virker korrekt
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        // Starter Avalonia som et normalt desktop-program
        // Det er dette der gør, at jeres GUI-vindue åbner
        .StartWithClassicDesktopLifetime(args);

    // Denne metode sætter Avalonia-frameworket op
    // Her fortæller vi:
    // - hvilken App der skal bruges (App.axaml / App.axaml.cs)
    // - hvilket styresystem programmet kører på
    // - hvilken font der bruges
    // - at fejl/logs kan skrives i output (til debugging)
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()   // Bruger App-klassen som hoved-app
            .UsePlatformDetect()         // Finder automatisk Windows / Mac / Linux
            .WithInterFont()             // Bruger standard font
            .LogToTrace();               // Logger info (kun til udvikling)
}