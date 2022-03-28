using Framework.Game.Server;
using Framework.Game;
using Shooter.Shared.Components;
namespace Shooter.Server
{
    public partial class MyServerPlayer : ServerPlayer
    {
        public MyServerPlayer(int id, IWorld world) : base(id, world)
        {
            this.AvaiablePlayerComponents.Add("body", new AssignedComponent(
                typeof(PlayerBodyComponent), "res://Assets/Player/PlayerBody.tscn"
            ));

            this.AvaiablePlayerComponents.Add("camera", new AssignedComponent(
                typeof(PlayerCameraComponent)
            ));
        }
    }
}