
using Godot;
using Framework;

namespace Shooter.Client.UI.Ingame
{
    public partial class MenuComponent : CanvasLayer, IChildComponent<Framework.Game.GameLogic>
    {
        public Framework.Game.GameLogic BaseComponent { get; set; }

        [Export]
        public NodePath DisconnectPath { get; set; }

        [Export]
        public NodePath SettingsPath { get; set; }

        [Export]
        public NodePath ClosePath { get; set; }

        public override void _EnterTree()
        {
            this.GetNode<Button>(this.DisconnectPath).Pressed += () =>
            {
                var component = BaseComponent as MyClientLogic;
                component.Disconnect();
            };

            this.GetNode<Button>(this.SettingsPath).Pressed += () =>
            {
                var component = BaseComponent as MyClientLogic;
                this.BaseComponent.Components.DeleteComponent<MenuComponent>();
                component.Components.AddComponent<GameSettings>("res://Game/Client/UI/Ingame/GameSettings.tscn");
            };

            this.GetNode<Button>(this.ClosePath).Pressed += () =>
            {
                var component = BaseComponent as MyClientLogic;
                this.BaseComponent.Components.DeleteComponent<MenuComponent>();
                Input.SetMouseMode(Input.MouseMode.Captured);
            };
        }

    }
}
