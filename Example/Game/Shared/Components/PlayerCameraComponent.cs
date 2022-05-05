using Framework.Physics;
using Framework.Game;

namespace Shooter.Shared.Components
{
    public partial class PlayerCameraComponent : CharacterCamera
    {
        public override void _EnterTree()
        {
            base._EnterTree();

            this.Far = 150;
            this.Near = 0.1f;
            this.DopplerTracking = DopplerTrackingEnum.PhysicsStep;
        }
    }
}
