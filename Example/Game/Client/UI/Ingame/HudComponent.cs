using Godot;
using System;
using Framework;


namespace Shooter.Client.UI.Ingame
{
    public partial class HudComponent : CanvasLayer, IChildComponent<Framework.Game.GameLogic>
    {
        public Framework.Game.GameLogic BaseComponent { get; set; }
    }
}
