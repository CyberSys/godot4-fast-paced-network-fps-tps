using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Client;

namespace Shooter.Client.UI.Welcome
{
	public partial class MapLoadingComponent : CanvasLayer, IComponent<ClientGameLogic>
	{
		public ClientGameLogic MainComponent { get; set; }

		[Export]
		public NodePath pathToProgressBar { get; set; }

		[Export]
		public NodePath pathToLoadingTextBox { get; set; }

		public override void _EnterTree()
		{
			base._EnterTree();
			this.MainComponent.loader.OnProgress += (string file, float process) =>
			{
				this.GetNode<Label>(pathToLoadingTextBox).Text = "Loading " + file;
				this.GetNode<ProgressBar>(pathToProgressBar).Value = process;
			};
		}
	}
}
