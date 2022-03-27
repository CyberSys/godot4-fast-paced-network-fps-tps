using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Framework.Network.Commands
{
    public struct ClientInitializer : INetSerializable
    {
        public uint GameTick;
        public int PlayerId;
        public Dictionary<string, string> ServerVars;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.GameTick);
            writer.Put(this.PlayerId);
            writer.Put(this.ServerVars);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.GameTick = reader.GetUInt();
            this.PlayerId = reader.GetInt();
            this.ServerVars = reader.GetDictonaryString();
        }
    }
}
