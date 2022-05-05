using System.Linq;
using Godot;
using System;
using Framework;
using Shooter.Server;
using Shooter.Client;
using Framework.Game;
public partial class SingleBootloader : Bootloader
{
    public enum BootloaderMode
    {
        Client,
        Server,
        Both
    }

    public static BootloaderMode currentMode = BootloaderMode.Both;

    private void createServerWindow(string name = "window")
    {
        Logger.LogDebug(this, "Load server..");

        var viewport = GD.Load<PackedScene>(ServerLogicScenePath).Instantiate<GameLogic>();
        viewport.Name = name;
        GetNode("box").AddChild(viewport);
    }

    private void createClientWindow(string name = "window")
    {
        Logger.LogDebug(this, "Load client..");

        var viewport = GD.Load<PackedScene>(ClientLogicScenePath).Instantiate<GameLogic>();
        viewport.Name = name;
        GetNode("box").AddChild(viewport);
    }

    public override void _Ready()
    {
        base._Ready();
        this.ProcessMode = ProcessModeEnum.Always;

        if (OS.GetCmdlineArgs().Contains("-client"))
        {
            currentMode = BootloaderMode.Client;
            GetTree().Root.Title = "Client Version";
            this.createClientWindow();
        }
        else if (OS.GetCmdlineArgs().Contains("-server"))
        {
            currentMode = BootloaderMode.Server;
            //  RenderingServer.RenderLoopEnabled = false;
            //DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled, 0);
            //Engine.TargetFps = 60;
            GetTree().Root.Title = "Server Version";

            this.createServerWindow();
        }
    }
}
