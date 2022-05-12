using System;
using Godot;
using Framework;
using Framework.Game;
using Framework.Network;
namespace Shooter.Client.UI.Welcome
{
    public partial class DebugMenuComponent : CanvasLayer, IChildComponent<GameLogic>
    {

        public GameLogic BaseComponent { get; set; }

        [Export]
        public NodePath LogMessagePath { get; set; }

        [Export]
        public NodePath FPSPath { get; set; }

        [Export]
        public NodePath PingPath { get; set; }

        [Export]
        public NodePath PackageLoosePath { get; set; }

        [Export]
        public NodePath PackageDataPath { get; set; }

        [Export]
        public NodePath IdleTimePath { get; set; }

        [Export]
        public NodePath PhysicsTimePath { get; set; }

        [Export]
        public NodePath TimerPath { get; set; }

        private ClientNetworkService netService;
        private Timer timer;

        private Label packageData, packageLosse, Ping, Fps, IdleTime, PhysicsTime;

        public override void _EnterTree()
        {
            Logger.OnLogMessage += (string message) =>
            {
                this.GetNode<Label>(this.LogMessagePath).Text = message;
            };

            var componnent = this.BaseComponent as IGameLogic;
            netService = componnent.Services.Get<ClientNetworkService>();

            this.packageData = this.GetNode<Label>(this.PackageDataPath);
            this.packageLosse = this.GetNode<Label>(this.PackageLoosePath);
            this.Ping = this.GetNode<Label>(this.PingPath);
            this.Fps = this.GetNode<Label>(this.FPSPath);
            this.IdleTime = this.GetNode<Label>(this.IdleTimePath);
            this.PhysicsTime = this.GetNode<Label>(this.PhysicsTimePath);

            timer = this.GetNode<Timer>(TimerPath);
        }

        public override void _Ready()
        {
            base._Ready();
            timer.Timeout += processStats;
            timer.Autostart = true;
            timer.Start();
        }

        public void processStats()
        {
            if (netService != null)
            {
                this.packageData.Text = "Send: " + (netService.BytesSended / 1000) + "kB/s, " + "Rec: " + (netService.BytesReceived / 1000) + "kB/s";
                this.packageLosse.Text = netService.PackageLoss + " (" + netService.PackageLossPercent + "%" + ")";
                this.Ping.Text = netService.Ping.ToString() + "ms";
            }

            this.Fps.Text = Engine.GetFramesPerSecond().ToString();
            this.IdleTime.Text = Math.Round(this.GetProcessDeltaTime() * 1000, 6) + "ms";
            this.PhysicsTime.Text = Math.Round(this.GetPhysicsProcessDeltaTime() * 1000, 6) + "ms";
        }
    }
}
