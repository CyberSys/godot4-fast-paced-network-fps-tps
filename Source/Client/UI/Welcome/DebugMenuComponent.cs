using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Client;
using Shooter.Client.Services;
namespace Shooter.Client.UI.Welcome
{
	public partial class DebugMenuComponent : CanvasLayer, IComponent<ClientGameLogic>
	{
		public ClientGameLogic MainComponent { get; set; }

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

		private float updateTimeLabel = 0.5f;

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

				var netService = this.MainComponent.Services.Get<ClientNetworkService>();
				if (netService != null)
				{
					this.GetNode<Label>(this.PackageDataPath).Text = "Send: " + (netService.bytesSended / 1000) + "kB/s, " + "Rec: " + (netService.bytesReceived / 1000) + "kB/s";
					this.GetNode<Label>(this.PackageLoosePath).Text = netService.packageLoss + " (" + netService.packageLossPercent + "%" + ")";
					this.GetNode<Label>(this.PingPath).Text = netService.ping.ToString() + "ms" + " (id: " + netService.MyId + ")";

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
