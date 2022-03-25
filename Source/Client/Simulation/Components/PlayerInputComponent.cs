using Shooter.Shared;
using Godot;
using System;

namespace Shooter.Client.Simulation.Components
{
    public partial class PlayerInputComponent : Node, IComponent<PlayerSimulation>
    {
        public bool canExecute = true;

        public PlayerSimulation MainComponent { get; set; }



        public PlayerInputs GetPlayerInput()
        {
            var camera = this.MainComponent.Components.Get<PlayerCameraComponent>();

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
