using Godot;
using LiteNetLib.Utils;
using System.Runtime.Serialization;

namespace Shooter.Shared.Network.Packages
{
    public struct PlayerUpdatePackage : INetSerializable, IPlayer
    {
        public PlayerTeam Team { get; set; }
        public ClientState State { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public int Latency { get; set; }

        [IgnoreDataMemberAttribute]
        public float DisconnectTime { get; set; }

        [IgnoreDataMemberAttribute]
        public uint latestInputTick { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((int)this.Team);
            writer.Put((int)this.State);
            writer.Put(this.Name);
            writer.Put(this.Id);
            writer.Put(this.Latency);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.Team = (PlayerTeam)reader.GetInt();
            this.State = (ClientState)reader.GetInt();
            this.Name = reader.GetString();
            this.Id = reader.GetInt();
            this.Latency = reader.GetInt();
        }
    }
}
