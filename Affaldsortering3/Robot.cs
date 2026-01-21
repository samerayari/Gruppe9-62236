using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Affaldsortering3;

// Denne fil er “robot-forbindelsen”
// Den bruges til at sende kommandoer fra GUI'en til robotten via netværk (TCP)
public class Robot(string ipAddress = "172.20.254.204", int dashboardPort = 29999, int urscriptPort = 30002)
{
    // 2 forbindelser (TCP):
    // 1) Dashboard porten (29999) = robot kommandoer som power on, stop, robotmode osv.
    // 2) URScript porten (30002) = sende selve robot-programmet/kommandoer (URScript)
    private readonly TcpClient _clientDashboard = new();
    private readonly TcpClient _clientUrscript = new();

    // Streams = “kanaler” hvor vi læser/skriver data til robotten
    private Stream _streamDashboard;
    private StreamReader _streamReaderDashboard;
    private Stream _streamUrscript;

    // Denne property spørger robotten om programmet kører lige nu
    // Den sender "running" til dashboard og læser svaret tilbage
    public bool ProgramRunning
    {
        get
        {
            if (_clientDashboard.Connected)
            {
                SendDashboard("running\n");
                return ReadLineDashboard() == "Program running: true";
            }

            return false;
        }
    }

    // Denne property fortæller om vi er forbundet korrekt til begge porte
    public bool Connected => _clientDashboard.Connected && _clientUrscript.Connected;

    // Denne property spørger robotten om dens mode (fx IDLE/RUNNING osv.)
    public string RobotMode
    {
        get
        {
            SendDashboard("robotmode\n");
            return ReadLineDashboard();
        }
    }

    // Connect() åbner forbindelsen til robotten
    // Først dashboard, bagefter URScript
    public void Connect()
    {
        // Opretter forbindelse til dashboard-porten
        _clientDashboard.Connect(ipAddress, dashboardPort);
        _streamDashboard = _clientDashboard.GetStream();

        // StreamReader gør det nemt at læse en hel linje fra robotten
        _streamReaderDashboard = new StreamReader(_streamDashboard, Encoding.ASCII);

        // Robotten sender en “velkomst-linje” på dashboard, den læser vi og ignorerer
        ReadLineDashboard();

        // Opretter forbindelse til URScript-porten
        _clientUrscript.Connect(ipAddress, urscriptPort);
        _streamUrscript = _clientUrscript.GetStream();
    }

    // PowerOn() tænder robotten via dashboard
    // Den venter indtil robotten siger den er i IDLE (klar)
    public async void PowerOn()
    {
        SendDashboard("power on\n");
        ReadLineDashboard(); // læser robot-svar (så stream ikke “fyldes”)

        // Venter på at robotten bliver klar
        while (RobotMode != "Robotmode: IDLE") await Task.Delay(1000);
    }

    // BrakeRelease() frigiver bremserne på robotten
    // Den venter indtil robotten siger RUNNING (klar til bevægelse)
    public async void BrakeRelease()
    {
        SendDashboard("brake release\n");
        ReadLineDashboard(); // læser robot-svar

        // Venter på at robotten er klar til at køre/bevæge sig
        while (RobotMode != "Robotmode: RUNNING") await Task.Delay(1000);
    }

    // Lukker begge forbindelser
    public void Disconnect()
    {
        _clientDashboard.Close();
        _clientUrscript.Close();
    }

    // Sender en kommando til dashboard-porten (tekst + \n)
    public void SendDashboard(string command)
    {
        _streamDashboard.Write(Encoding.ASCII.GetBytes(command));
    }

    // Sender URScript direkte til robotten (på URScript-porten)
    public void SendUrscript(string program)
    {
        _streamUrscript.Write(Encoding.ASCII.GetBytes(program));
    }

    // Læser en URScript-fil fra projektmappen og sender den til robotten
    // Det er fx "robot.script" som starter hele robot-programmet
    public void SendUrscriptFile(string path)
    {
        var program = File.ReadAllText(path) + Environment.NewLine;
        SendUrscript(program);
    }

    // Læser én linje svar fra dashboard (robotten svarer ofte i tekst)
    public string ReadLineDashboard()
    {
        return _streamReaderDashboard.ReadLine();
    }

    // NYE FUNKTIONER (stop knapper i GUI)
    // De ændrer ikke robot.script - de sender bare stop-kommandoer via netværket

    // Normal stop: stopper programmet “pænt” via dashboard
    public string StopProgram()
    {
        SendDashboard("stop\n");
        return ReadLineDashboard() ?? "(no reply)";
    }

    // EmergencyStop: stopper programmet og stopper bevægelse hurtigt
    // (software-nødstop - det er ikke den fysiske E-stop knap)
    public void EmergencyStop()
    {
        // Stop program via dashboard
        SendDashboard("stop\n");
        ReadLineDashboard(); // læser robot-svar

        // Stop robot-bevægelse hurtigt (decel)
        SendUrscript("stopj(2)\n");
    }
}