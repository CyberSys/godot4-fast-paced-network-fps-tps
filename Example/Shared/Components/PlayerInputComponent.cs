using Godot;
using Framework.Input;
using Framework.Network;
using Framework;

namespace Shooter.Shared.Components
{
    public partial class PlayerInputComponent : Node, IChildInputComponent
    {
        public IBaseComponent BaseComponent { get; set; }

        public GeneralPlayerInput LastPlayerInput { get; set; }

        public string GetComponentName()
        {
            return "input";
        }

        public GeneralPlayerInput GetPlayerInput()
        {
            var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

            var horiz = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            var vert = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
            var input = new GeneralPlayerInput();

            input.SetInput("Forward", vert > 0);
            input.SetInput("Back", vert < 0);
            input.SetInput("Right", horiz > 0);
            input.SetInput("Left", horiz < 0);
            input.SetInput("Jump", Input.IsActionPressed("jump"));
            input.SetInput("Crouch", Input.IsActionPressed("move_crouch"));
            input.SetInput("Fire", Input.IsActionPressed("attack"));

            input.ViewDirection = camera != null ? camera.getViewRotation() : Quaternion.Identity;

            return input;
        }
    }
}
