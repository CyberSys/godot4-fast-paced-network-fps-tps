using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;

namespace Shooter.Client
{
    public partial class MyLocalPlayer : LocalPlayer
    {
        public MyLocalPlayer() : base()
        {
            this.AddAvaiableComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
            this.AddAvaiableComponent<PlayerCameraComponent>();
            this.AddAvaiableComponent<PlayerFootstepComponent>();
            this.AddAvaiableComponent<PlayerInputComponent>();
            this.AddAvaiableComponent<PlayerWeaponComponent>("res://Assets/Weapons/WeaponHolder.tscn");

            this.AvaiableInputs.AddRange(new string[]{
                "Forward", "Back", "Left", "Right", "Jump", "Crouch", "Shifting", "Fire"
            });
        }
    }
}