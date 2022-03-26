using Godot;
using LiteNetLib.Utils;
using System.Runtime.Serialization;
using Framework.Game;

namespace Framework.Network.Commands
{

    public struct PlayerUpdate : INetSerializable
    {
        public PlayerTeam Team { get; set; }
        public PlayerConnectionState State { get; set; }
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
            this.State = (PlayerConnectionState)reader.GetInt();
            this.Name = reader.GetString();
            this.Id = reader.GetInt();
            this.Latency = reader.GetInt();
        }
    }
}
