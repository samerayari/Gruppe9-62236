// Gør det muligt at lave en Avalonia-app
using Avalonia;

// Bruges til at styre hvordan appen starter (desktop-app)
using Avalonia.Controls.ApplicationLifetimes;

// Bruges til at indlæse XAML-filer (fx App.axaml)
using Avalonia.Markup.Xaml;

// Navnet på vores projekt
namespace Affaldsortering3;

// Denne klasse er selve appen
public partial class App : Application
{
    // Kører når appen starter
    // Loader App.axaml (udseende og tema)
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Kører når frameworket er klar
    // Her bestemmer vi hvilket vindue der skal åbnes først
    public override void OnFrameworkInitializationCompleted()
    {
        // Tjekker om appen kører som en desktop-app
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Åbner MainWindow (det vindue man ser)
            desktop.MainWindow = new MainWindow();
        }

        // Afslutter opstarten korrekt
        base.OnFrameworkInitializationCompleted();
    }
}