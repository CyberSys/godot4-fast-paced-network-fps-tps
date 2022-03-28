using System;
using Godot;
using Framework;
using Framework.Game;
using Framework.Network.Services;
namespace Shooter.Client.UI.Welcome
{
    public partial class DebugMenuComponent : CanvasLayer, IChildComponent
    {
        public IBaseComponent BaseComponent { get; set; }

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
        public float updateTimeLabel = 0.5f;

        private float currentUpdateTime = 0f;

        public override void _EnterTree()
        {
            Logger.OnLogMessage += (string message) =>
        {
            this.GetNode<Label>(this.LogMessagePath).Text = message;
        };
        }


        public override void _Process(float delta)
        {
            base._Process(delta);

            if (currentUpdateTime >= updateTimeLabel)
            {
                var componnent = this.BaseComponent as IGameLogic;
                var netService = componnent.Services.Get<ClientNetworkService>();
                if (netService != null)
                {
                    this.GetNode<Label>(this.PackageDataPath).Text = "Send: " + (netService.bytesSended / 1000) + "kB/s, " + "Rec: " + (netService.bytesReceived / 1000) + "kB/s";
                    this.GetNode<Label>(this.PackageLoosePath).Text = netService.packageLoss + " (" + netService.packageLossPercent + "%" + ")";
                    this.GetNode<Label>(this.PingPath).Text = netService.ping.ToString() + "ms";

                }

                this.GetNode<Label>(this.FPSPath).Text = Engine.GetFramesPerSecond().ToString() + " (" + Math.Round(delta, 3) + "ms)";
                currentUpdateTime = 0f;
            }
            else
            {
                currentUpdateTime += delta;
            }
        }
    }
}
