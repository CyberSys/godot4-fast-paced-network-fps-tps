using Framework;
using Framework.Network;
using Framework.Physics;
using Godot;
using System;
using Framework.Network.Commands;
using Framework.Game;
using LiteNetLib.Utils;

namespace Shooter.Shared.Components
{
    public struct FootStepsPackage : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {

        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {

        }
    }

    public partial class PlayerFootstepComponent : Node, IChildNetworkSyncComponent<FootStepsPackage>
    {
        public IBaseComponent BaseComponent { get; set; }

        public string GetComponentName()
        {
            return "footsteps";
        }

        public void ApplyNetworkState(FootStepsPackage package)
        {

        }

        public FootStepsPackage GetNetworkState()
        {
            return new FootStepsPackage { };
        }
    }
}
