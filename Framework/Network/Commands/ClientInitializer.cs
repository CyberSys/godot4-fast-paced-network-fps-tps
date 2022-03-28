using System.Collections.Generic;
using LiteNetLib.Utils;
using Framework.Game.Server;

namespace Framework.Network.Commands
{
    public struct ClientInitializer : INetSerializable
    {
        public uint GameTick;
        public int PlayerId;
        public ServerVars ServerVars;


        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.GameTick);
            writer.Put(this.PlayerId);
            this.ServerVars.Serialize(writer);
        }


        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.GameTick = reader.GetUInt();
            this.PlayerId = reader.GetInt();
            this.ServerVars.Deserialize(reader);
        }
    }
}
