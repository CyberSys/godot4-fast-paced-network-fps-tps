using System.Collections.Generic;
using Godot;
using Framework.Input;
using Framework.Network;
using Framework;
using Framework.Physics;
namespace Shooter.Shared.Components
{
    public partial class PlayerInputComponent : Node, IChildInputComponent
    {
        public IBaseComponent BaseComponent { get; set; }

        public void Tick(float delta)
        {
        }

        public GeneralPlayerInput GetPlayerInput()
        {
            var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

            var input = new GeneralPlayerInput();
            var activeKeys = new Dictionary<string, bool>();

            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                activeKeys = new Dictionary<string, bool>{
                    {"Forward", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_forward", Godot.Key.W)},
                    {"Back", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_backward", Godot.Key.S)},
                    {"Right", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_right", Godot.Key.D)},
                    {"Left", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_left", Godot.Key.A)},
                    {"Jump", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_jump", Godot.Key.Space)},
                    {"Crouch", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_crouch", Godot.Key.Ctrl)},
                    {"Shifting", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_shift", Godot.Key.Shift)},
                    {"Fire",  Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_attack", Godot.MouseButton.Left)},
                };
            }

            input.ViewDirection = camera != null ? camera.getViewRotation() : Quaternion.Identity;
            input.Apply((this.BaseComponent as PhysicsPlayer).AvaiableInputs, activeKeys);

            return input;
        }
    }
}
