using Godot;
using Framework.Input;
using Framework.Network;
using Framework;
using Framework.Game.Client;
using LiteNetLib.Utils;
using Framework.Physics;
namespace Shooter.Shared.Components
{

    public struct PlayerWeaponPackage : INetSerializable
    {
        public int WeaponIndex { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(WeaponIndex);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            WeaponIndex = reader.GetInt();
        }
    }

    public partial class PlayerWeaponComponent : Node3D, IChildNetworkSyncComponent<PlayerWeaponPackage>, IPlayerComponent
    {
        public IBaseComponent BaseComponent { get; set; }

        [Export]
        public Vector3 weaponHolderOffset = new Godot.Vector3(1f, 0f, 0f);


        public void Tick(float delta)
        {
            var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
            if (body != null)
            {
                var player = this.BaseComponent as PhysicsPlayer;

                var transform = this.Transform;
                transform.origin = body.Transform.origin + weaponHolderOffset;
                this.Transform = transform;

                this.Transform = this.Transform.LookingAt(body.Transform.origin, Vector3.Up);
            }
        }

        public void ApplyNetworkState(PlayerWeaponPackage package)
        {

        }

        public PlayerWeaponPackage GetNetworkState()
        {
            return new PlayerWeaponPackage
            {

            };
        }
    }
}
