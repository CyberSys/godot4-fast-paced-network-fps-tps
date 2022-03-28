using Godot;
using Framework.Input;
using Framework.Network;
using Framework;

namespace Shooter.Shared.Components
{
    public partial class PlayerInputComponent : Node, IInputable
    {
        public bool canExecute = true;

        public IBaseComponent BaseComponent { get; set; }

        public IPlayerInput GetPlayerInput()
        {
            var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

            var horiz = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            var vert = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");

            return new PlayerInputs
            {
                Forward = vert > 0,
                Back = vert < 0,
                Right = horiz > 0,
                Left = horiz < 0,
                ViewDirection = camera != null ? camera.getViewRotation() : Quaternion.Identity,
                Jump = Input.IsActionPressed("jump"),
                Crouch = Input.IsActionPressed("move_crouch"),
                Fire = Input.IsActionPressed("attack"),
            };

        }
    }
}
