using Framework.Physics;

namespace Shooter.Shared.Components
{
    public partial class PlayerCameraComponent : PhysicsPlayerCamera
    {
        public override void _EnterTree()
        {
            base._EnterTree();

            this.Far = 150;
            this.Near = 0.1f;
            this.DopplerTracking = DopplerTrackingEnum.PhysicsStep;

            this.SetCullMaskValue(2, false);
        }
    }
}
