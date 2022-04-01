using Framework.Game.Server;
using Framework.Game;
using Shooter.Shared.Components;
namespace Shooter.Server
{
    public partial class MyServerPlayer : ServerPlayer
    {
        public MyServerPlayer() : base()
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

        public override void Tick(float delta)
        {
            base.Tick(delta);
        }
    }
}