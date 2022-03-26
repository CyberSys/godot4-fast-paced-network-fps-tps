
using LiteNetLib.Utils;

namespace Framework.Network.Commands
{
    public struct ServerInitializer : INetSerializable
    {
        public int handshake { get; set; }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.handshake);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.handshake = reader.GetInt();
        }
    }
}
