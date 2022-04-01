using Godot;
using Framework.Input;
using Framework.Network;
using Framework;
using Framework.Game.Client;
using LiteNetLib.Utils;

namespace Shooter.Shared.Components
{

    public struct PlayerWeaponPackage : INetSerializable
    {
        public int WeaponIndex { get; set; }

        public bool Equals(FootStepsPackage compareObj)
        {
            return this.GetHashCode() == compareObj.GetHashCode();
        }

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

        public GeneralPlayerInput LastPlayerInput { get; set; }

        [Export]
        public Vector3 weaponHolderOffset = new Godot.Vector3(1f, 0f, 0f);

        public string GetComponentName()
        {
            return "weapon";
        }

        public void Tick(float delta)
        {
            var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
            if (body != null)
            {
                var transform = body.Transform;
                transform.origin += weaponHolderOffset;
                this.Transform = transform;
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
