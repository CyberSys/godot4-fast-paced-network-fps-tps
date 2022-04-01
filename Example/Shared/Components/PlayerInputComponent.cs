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

        public GeneralPlayerInput LastPlayerInput { get; set; }



        public void Tick(float delta)
        {

        }

        public GeneralPlayerInput GetPlayerInput()
        {
            var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

            var horiz = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            var vert = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
            var input = new GeneralPlayerInput();

            var activeKeys = new Dictionary<string, bool>{
                {"Forward", vert > 0},
                {"Back", vert < 0},
                {"Right", horiz > 0},
                {"Left", horiz < 0},
                {"Jump",Input.IsActionPressed("jump")},
                {"Crouch", Input.IsActionPressed("move_crouch")},
                {"Shifting", Input.IsActionPressed("move_shift")},
                {"Fire", Input.IsActionPressed("attack")},
            };

            input.ViewDirection = camera != null ? camera.getViewRotation() : Quaternion.Identity;
            input.Apply((this.BaseComponent as PhysicsPlayer).AvaiableInputs, activeKeys);

            return input;
        }
    }
}
