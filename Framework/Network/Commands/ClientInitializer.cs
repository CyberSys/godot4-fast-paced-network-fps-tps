using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Framework.Network.Commands
{
    public struct ClientInitializer : INetSerializable
    {
        public uint GameTick { get; set; }
        public int PlayerId { get; set; }
        public Dictionary<string, string> ServerVars { get; set; }

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
