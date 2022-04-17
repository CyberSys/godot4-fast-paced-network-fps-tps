using Godot;
using System;
using Framework;


namespace Shooter.Client.UI.Ingame
{
	public partial class HudComponent : CanvasLayer, IChildComponent
	{
		public IBaseComponent BaseComponent { get; set; }
	}
}
