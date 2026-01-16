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
    private readonly AppDbContext _db = new();
    private readonly PasswordHasher _hasher = new();
    private readonly AccountService _accountService;
    private readonly LoginEventService _loginEventService;

    // Robot
    private readonly Robot _robot;

    // UI collections
    public ObservableCollection<string> UiLog { get; } = new();
    public ObservableCollection<Account> Users { get; } = new();
    public ObservableCollection<LoginEvent> LoginEvents { get; } = new();

    // State
    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowLogin));
            OnPropertyChanged(nameof(ShowApp));
        }
    }

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set { _isAdmin = value; OnPropertyChanged(); }
    }

    public bool ShowLogin => !IsLoggedIn;
    public bool ShowApp => IsLoggedIn;

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

    private string _loginError = "";
    public string LoginError
    {
        get => _loginError;
        set { _loginError = value; OnPropertyChanged(); }
    }

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

    private string _userCreateError = "";
    public string UserCreateError
    {
        get => _userCreateError;
        set { _userCreateError = value; OnPropertyChanged(); }
    }

    private string _robotStatus = "";
    public string RobotStatus
    {
        get => _robotStatus;
        set { _robotStatus = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _accountService = new AccountService(_db, _hasher);
        _loginEventService = new LoginEventService(_db);

        _robot = new Robot();

        TryConnectRobot();
        _ = InitAsync();
    }

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

    private void AddLog(string message)
    {
        UiLog.Insert(0, $"{DateTime.Now:dd-MM-yy HH.mm.ss} | {message}");
    }

    
    // Login / Logout
    
    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        LoginError = "";

        try
        {
            var (ok, isAdmin) =
                await _accountService.ValidateAsync(LoginUsername, LoginPassword);

            if (!ok)
            {
                await _loginEventService.AddAsync(LoginUsername, false, "Wrong credentials");
                LoginError = "Forkert username eller password.";
                AddLog($"LOGIN FAIL: {LoginUsername}");
                return;
            }

            IsLoggedIn = true;
            IsAdmin = isAdmin;

            await _loginEventService.AddAsync(
                LoginUsername, true, $"Logged in. Admin={IsAdmin}");

            AddLog($"{LoginUsername} logged in. Admin={IsAdmin}");

            await RefreshAdminDataAsync();
        }
        catch (Exception ex)
        {
            LoginError = ex.Message;
            AddLog("Login error: " + ex.Message);
        }
        finally
        {
            LoginPassword = "";
        }
    }

    private void Logout_Click(object? sender, RoutedEventArgs e)
    {
        AddLog("Logged out.");

        IsLoggedIn = false;
        IsAdmin = false;

        LoginUsername = "";
        LoginPassword = "";
        LoginError = "";

        Users.Clear();
        LoginEvents.Clear();
    }

  
    // Admin
   
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

    private async void CreateUser_Click(object? sender, RoutedEventArgs e)
    {
        UserCreateError = "";

        if (!IsAdmin)
        {
            UserCreateError = "Kun admin kan oprette brugere.";
            return;
        }

        try
        {
            await _accountService.CreateUserAsync(
                NewUserUsername, NewUserPassword, NewUserIsAdmin);

            AddLog($"User created: {NewUserUsername} (Admin={NewUserIsAdmin})");

            NewUserUsername = "";
            NewUserPassword = "";
            NewUserIsAdmin = false;

            await RefreshAdminDataAsync();
        }
        catch (Exception ex)
        {
            UserCreateError = ex.Message;
            AddLog("Create user error: " + ex.Message);
        }
    }

 

    private void ClearLog_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsAdmin) return;
        UiLog.Clear();
        AddLog("Log cleared.");
    }

    
    // Robot
   
    private void RunRobot_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
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

    // NY: Stop (normal)
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

    // NY: Emergency stop (software)
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
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
