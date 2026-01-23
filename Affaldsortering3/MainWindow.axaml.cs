
// Fil: MainWindow.axaml.cs
// Denne fil er “hjernen” bag vores GUI (MainWindow.axaml).
// Her ligger alt der sker når man klikker på knapperne: login, logout, opret bruger, robot-knapper og log.
// DataContext = this gør at XAML kan bruge vores properties (fx IsLoggedIn, IsAdmin, UiLog osv.)

using System;
using System.Collections.ObjectModel; 
using System.ComponentModel;      
using System.Runtime.CompilerServices; 
using System.Threading.Tasks;       
using Avalonia.Controls;           
using Avalonia.Interactivity;      
using Affaldsortering3.Data;          

namespace Affaldsortering3; 

public partial class MainWindow : Window, INotifyPropertyChanged
{
    // Database + services
    // _db = forbindelsen til databasen (app.db)
    // _hasher = laver password om til hash + salt (så vi ikke gemmer "rent" password)
    // _accountService = hjælper med brugere (opret, valider login, hent brugere)
    // _loginEventService = gemmer login-hændelser (success/fail) i databasen
    private readonly AppDbContext _db = new();
    private readonly PasswordHasher _hasher = new();
    private readonly AccountService _accountService;
    private readonly LoginEventService _loginEventService;

    // Robot
    // _robot = vores robot-klasse der kan connecte og sende URScript-kommandoer
    private readonly Robot _robot;

    // UI collections
    // UiLog = tekst-liste der vises i Database-fanen (Systemhistorik)
    // Users = liste over brugere (kan bruges i admin-delen)
    // LoginEvents = liste over login-hændelser (kan bruges i admin-delen)
    public ObservableCollection<string> UiLog { get; } = new();
    public ObservableCollection<Account> Users { get; } = new();
    public ObservableCollection<LoginEvent> LoginEvents { get; } = new();

    // State
    // IsLoggedIn styrer om login-skærmen eller app-skærmen vises
    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            OnPropertyChanged();                 // opdaterer IsLoggedIn i GUI
            OnPropertyChanged(nameof(ShowLogin)); // opdaterer ShowLogin
            OnPropertyChanged(nameof(ShowApp));   // opdaterer ShowApp
        }
    }

    // IsAdmin styrer om admin-faner/knapper vises (Users + Database + Clear log)
    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set { _isAdmin = value; OnPropertyChanged(); }
    }

    // “Hjælpe”-properties til XAML:
    // ShowLogin = vis login når man IKKE er logget ind
    // ShowApp = vis app når man ER logget ind
    public bool ShowLogin => !IsLoggedIn;
    public bool ShowApp => IsLoggedIn;

    // Login felter (bundet til TextBox i XAML)
    private string _loginUsername = "";
    public string LoginUsername
    {
        get => _loginUsername;
        set { _loginUsername = value; OnPropertyChanged(); }
    }

    private string _loginPassword = "";
    public string LoginPassword
    {
        get => _loginPassword;
        set { _loginPassword = value; OnPropertyChanged(); }
    }

    // Fejltekst på login-skærmen
    private string _loginError = "";
    public string LoginError
    {
        get => _loginError;
        set { _loginError = value; OnPropertyChanged(); }
    }

    // Admin: opret bruger felter (bundet til Users-fanen)
    private string _newUserUsername = "";
    public string NewUserUsername
    {
        get => _newUserUsername;
        set { _newUserUsername = value; OnPropertyChanged(); }
    }

    private string _newUserPassword = "";
    public string NewUserPassword
    {
        get => _newUserPassword;
        set { _newUserPassword = value; OnPropertyChanged(); }
    }

    private bool _newUserIsAdmin;
    public bool NewUserIsAdmin
    {
        get => _newUserIsAdmin;
        set { _newUserIsAdmin = value; OnPropertyChanged(); }
    }

    // Fejltekst ved “Create user”
    private string _userCreateError = "";
    public string UserCreateError
    {
        get => _userCreateError;
        set { _userCreateError = value; OnPropertyChanged(); }
    }

    // Tekst der vises på Robot-fanen (status for forbindelse)
    private string _robotStatus = "";
    public string RobotStatus
    {
        get => _robotStatus;
        set { _robotStatus = value; OnPropertyChanged(); }
    }

    // Bruges så GUI automatisk opdaterer når properties ændres
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent(); // loader XAML-designet
        DataContext = this;    // gør at XAML kan “binde” til properties i denne klasse

        // Vi laver services, så vi kan arbejde med databasen på en pæn måde
        _accountService = new AccountService(_db, _hasher);
        _loginEventService = new LoginEventService(_db);

        // Robot-objektet bliver lavet her
        _robot = new Robot();

        // Forsøger at connecte robotten med det samme når programmet starter
        TryConnectRobot();

        // Starter init af databasen (async)
        _ = InitAsync();
    }

    // Init: sikrer at databasen findes og at admin-bruger findes
    private async Task InitAsync()
    {
        try
        {
            await _accountService.EnsureDbAsync();
            AddLog("DB OK. Default admin: admin/admin");
        }
        catch (Exception ex)
        {
            AddLog("DB error: " + ex.Message);
        }
    }

    // Prøver at forbinde til robotten og sætter status-tekst
    private void TryConnectRobot()
    {
        try
        {
            _robot.Connect();
            RobotStatus = "Robot: Connected";
            AddLog("Robot connected.");
        }
        catch (Exception ex)
        {
            RobotStatus = "Robot: Not connected";
            AddLog("Robot connection error: " + ex.Message);
        }
    }

    // Lægger en ny linje i loggen (øverst) med tidspunkt
    private void AddLog(string message)
    {
        UiLog.Insert(0, $"{DateTime.Now:dd-MM-yy HH.mm.ss} | {message}");
    }

    
    // Login / Logout
    
    // Kører når man trykker “Login” på login-skærmen
    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        LoginError = "";

        try
        {
            // Tjekker i databasen om username + password passer
            var (ok, isAdmin) =
                await _accountService.ValidateAsync(LoginUsername, LoginPassword);

            if (!ok)
            {
                // Gemmer i databasen at login fejlede
                await _loginEventService.AddAsync(LoginUsername, false, "Wrong credentials");
                LoginError = "Forkert username eller password.";
                AddLog($"LOGIN FAIL: {LoginUsername}");
                return;
            }

            // Login lykkedes: vi skifter visning fra login til app
            IsLoggedIn = true;
            IsAdmin = isAdmin;

            // Gemmer i databasen at login lykkedes
            await _loginEventService.AddAsync(
                LoginUsername, true, $"Logged in. Admin={IsAdmin}");

            AddLog($"{LoginUsername} logged in. Admin={IsAdmin}");

            // Hvis admin, så henter vi admin-data (brugere + events)
            await RefreshAdminDataAsync();
        }
        catch (Exception ex)
        {
            LoginError = ex.Message;
            AddLog("Login error: " + ex.Message);
        }
        finally
        {
            // Vi nulstiller password-feltet efter login
            LoginPassword = "";
        }
    }

    // Kører når man trykker “Logout”
    private void Logout_Click(object? sender, RoutedEventArgs e)
    {
        AddLog("Logged out.");

        // Skifter tilbage til login-visning og fjerner admin-rettigheder
        IsLoggedIn = false;
        IsAdmin = false;

        // Rydder felter og fejl
        LoginUsername = "";
        LoginPassword = "";
        LoginError = "";

        // Rydder lister (så admin-data ikke bliver hængende)
        Users.Clear();
        LoginEvents.Clear();
    }

  
    // Admin
   
    // Henter admin-data fra databasen (kun hvis IsAdmin = true)
    private async Task RefreshAdminDataAsync()
    {
        if (!IsAdmin) return;

        Users.Clear();
        foreach (var u in await _accountService.GetUsersAsync())
            Users.Add(u);

        LoginEvents.Clear();
        foreach (var ev in await _loginEventService.GetLatestAsync())
            LoginEvents.Add(ev);
    }

    // Kører når admin trykker “Create user”
    private async void CreateUser_Click(object? sender, RoutedEventArgs e)
    {
        UserCreateError = "";

        // Ekstra sikkerhed: kun admin må oprette
        if (!IsAdmin)
        {
            UserCreateError = "Kun admin kan oprette brugere.";
            return;
        }

        try
        {
            // Opretter bruger i databasen (hash + salt sker i AccountService)
            await _accountService.CreateUserAsync(
                NewUserUsername, NewUserPassword, NewUserIsAdmin);

            AddLog($"User created: {NewUserUsername} (Admin={NewUserIsAdmin})");

            // Rydder felterne efter oprettelse
            NewUserUsername = "";
            NewUserPassword = "";
            NewUserIsAdmin = false;

            // Opdaterer admin-data igen (så man kan se ændringer)
            await RefreshAdminDataAsync();
        }
        catch (Exception ex)
        {
            UserCreateError = ex.Message;
            AddLog("Create user error: " + ex.Message);
        }
    }

    // Admin-knap til at rydde UI-loggen
    private void ClearLog_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsAdmin) return;
        UiLog.Clear();
        AddLog("Log cleared.");
    }

    
    // Robot
   
    // Kører når man trykker “Start Robot”
    // Sender robot.script filen til robotten, så robotprogrammet starter
    private void RunRobot_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Hvis robot ikke er connected, prøv igen
            if (!_robot.Connected)
                TryConnectRobot();

            _robot.SendUrscriptFile("robot.script");
            AddLog("Robot: robot.script sendt.");
        }
        catch (Exception ex)
        {
            AddLog("Robot error: " + ex.Message);
        }
    }

    // Kører når man trykker “Power On”
    private void PowerOn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _robot.PowerOn();
            AddLog("Robot: power on.");
        }
        catch (Exception ex)
        {
            AddLog("PowerOn error: " + ex.Message);
        }
    }

    // Kører når man trykker “Brake Release”
    private void BrakeRelease_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _robot.BrakeRelease();
            AddLog("Robot: brake release.");
        }
        catch (Exception ex)
        {
            AddLog("BrakeRelease error: " + ex.Message);
        }
    }

    // Stop-knap: stopper programmet “normalt”
    private void Stop_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var reply = _robot.StopProgram();
            AddLog("Robot STOP pressed. Reply: " + reply);
        }
        catch (Exception ex)
        {
            AddLog("Stop error: " + ex.Message);
        }
    }

    // Emergency stop: “hård” stop via software
    private void EmergencyStop_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _robot.EmergencyStop();
            AddLog("!!! EMERGENCY STOP pressed !!!");
        }
        catch (Exception ex)
        {
            AddLog("Emergency stop error: " + ex.Message);
        }
    }

    // INotifyPropertyChanged
    // Dette er det der “fortæller” GUI’en: “nu har en værdi ændret sig”
    // Så opdaterer Avalonia automatisk det, der er bundet i XAML
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

