using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;

namespace Shooter.Client
{
    public partial class MyLocalPlayer : LocalPlayer
    {
        public MyLocalPlayer() : base()
        {
            this.AvaiablePlayerComponents.Add("body", new AssignedComponent(
                typeof(PlayerBodyComponent), "res://Assets/Player/PlayerBody.tscn"
            ));

            this.AvaiablePlayerComponents.Add("camera", new AssignedComponent(
                typeof(PlayerCameraComponent)
            ));

            this.AvaiablePlayerComponents.Add("footsteps", new AssignedComponent(
                typeof(PlayerFootstepComponent)
            ));

            this.AvaiablePlayerComponents.Add("input", new AssignedComponent(
                typeof(PlayerInputComponent)
            ));
        }
    }
}