using System.Diagnostics;
using System.Linq;
using Godot;
using System;
using Framework;
using Framework.Game.Server;
using Framework.Game.Client;
using Shooter.Server;
using Shooter.Client;
using Shooter;
public partial class Bootloader : Node
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

        var scene = new MyServerLogic();
        scene.Name = name;
        scene.ProcessMode = ProcessModeEnum.Always;

        GetNode("box").AddChild(scene);
    }

    private void createClientWindow(string name = "window")
    {
        Logger.LogDebug(this, "Load client..");

        var scene = new MyClientLogic();
        scene.Name = name;
        GetNode("box").AddChild(scene);
    }

    public override void _Ready()
    {
        base._Ready();

        GD.Load<CSharpScript>("res://Shared/MyGameLevel.cs");
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
            GD.Print("Run server");
            //  RenderingServer.RenderLoopEnabled = false;
            //DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled, 0);
            //Engine.TargetFps = 60;
            GetTree().Root.Title = "Server Version";

            this.createServerWindow();
        }
    }
}
