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
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
        }
    }
}