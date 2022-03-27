using Godot;
using LiteNetLib.Utils;
using System;
namespace Framework.Network.Commands
{
    public struct WorldHeartbeat : INetSerializable
    {
        // The world tick this data represents.
        public uint WorldTick;

        // The last world tick the server acknowledged for you.
        // The client should use this to determine the last acked input, as well as to compute
        // its relative simulation offset.
        public uint YourLatestInputTick;

        // States for all active players.
        public PlayerState[] PlayerStates;

        public PlayerUpdate[] PlayerUpdates;

        public void Serialize(NetDataWriter writer)
        {

            writer.Put(this.WorldTick);
            writer.Put(this.YourLatestInputTick);

            writer.PutArray<PlayerState>(this.PlayerStates);
            writer.PutArray<PlayerUpdate>(this.PlayerUpdates);

        }

        public void Deserialize(NetDataReader reader)
        {
            this.WorldTick = reader.GetUInt();
            this.YourLatestInputTick = reader.GetUInt();
            this.PlayerStates = reader.GetArray<PlayerState>();
            this.PlayerUpdates = reader.GetArray<PlayerUpdate>();

        }
    }
}