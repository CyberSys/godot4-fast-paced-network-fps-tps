using Framework;
using Framework.Network;
using Godot;
using Framework.Utils;
using Framework.Physics;
using Framework.Game.Client;

namespace Shooter.Shared.Components
{
    /*
    public enum CameraMode
    {
        FPS,
        TPS,
        Follow,
        Server,
        Dugeon
    }
    public partial class PlayerCameraComponent : Camera3D, IPlayerComponent
    {
        private DoubleBuffer<Vector3> positionBuffer = new DoubleBuffer<Vector3>();

        public void PlayerPositionUpdated()
        {
            var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
            if (body != null)
            {
                var targetPos = body.Transform.origin + cameraOffset + Vector3.Up * body.getCrouchingHeight();
                positionBuffer.Push(targetPos);
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            this.InternalProcess(delta);
        }

        /// <inheritdoc />
        internal virtual void InternalProcess(float delta)
        {

        }
    }
    */
}
