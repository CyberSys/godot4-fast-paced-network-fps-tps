using System.Linq;
using Godot;
using LiteNetLib.Utils;
using System.Runtime.Serialization;
using Framework.Game;
using System;
using System.Collections.Generic;

namespace Framework.Network.Commands
{
    public struct PlayerUpdate : INetSerializable
    {
        public PlayerTeam Team;
        public PlayerConnectionState State;
        public string PlayerName;
        public int Id;
        public int Latency;

        public string[] RequiredComponents;
        public string[] RequiredPuppetComponents;


        [IgnoreDataMemberAttribute]
        public float DisconnectTime;

        [IgnoreDataMemberAttribute]
        public uint LatestInputTick;

        public void Serialize(NetDataWriter writer)
        {

            writer.Put(this.Id);
            writer.Put(Convert.ToInt32(this.Team));
            writer.Put((int)this.State);
            writer.Put(this.PlayerName);
            writer.Put(this.Latency);
            writer.PutArray(this.RequiredComponents);
            writer.PutArray(this.RequiredPuppetComponents);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.Id = reader.GetInt();
            this.Team = (PlayerTeam)reader.GetInt();
            this.State = (PlayerConnectionState)reader.GetInt();
            this.PlayerName = reader.GetString();
            this.Latency = reader.GetInt();
            this.RequiredComponents = reader.GetStringArray();
            this.RequiredPuppetComponents = reader.GetStringArray();
        }
    }
}
