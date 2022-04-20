using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;

namespace Shooter.Client
{
    public partial class MyPuppetPlayer : PuppetPlayer
    {
        public MyPuppetPlayer() : base()
        {
            this.AddAvaiableComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
            this.AddAvaiableComponent<PlayerCameraComponent>();
            this.AddAvaiableComponent<PlayerFootstepComponent>();
            //  this.AddAvaiableComponent<PlayerInputComponent>();
            this.AddAvaiableComponent<PlayerWeaponComponent>("res://Assets/Weapons/WeaponHolder.tscn");
        }
    }
}