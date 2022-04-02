using Godot;
using Framework.Network;
using Framework;
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

	public partial class PlayerWeaponComponent : SubViewportContainer, IChildNetworkSyncComponent<PlayerWeaponPackage>, IPlayerComponent
	{
		public IBaseComponent BaseComponent { get; set; }

		[Export]
		NodePath cameraPath;

		public override void _EnterTree()
		{
			base._EnterTree();

		}
		public void Tick(float delta)
		{
			var body = this.BaseComponent.Components.Get<PlayerCameraComponent>();
			if (body != null)
			{
				this.GetNode<Camera3D>(cameraPath).GlobalTransform = body.GlobalTransform;
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
