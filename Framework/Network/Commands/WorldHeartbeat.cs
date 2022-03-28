using Godot;
using LiteNetLib.Utils;
using System;
namespace Framework.Network.Commands
{
    /// <summary>
    /// The world heartbeat structure for network syncronisation
    /// </summary>
    public struct WorldHeartbeat : INetSerializable
    {
        /// <summary>
        /// The world tick this data represents.
        /// </summary>
        public uint WorldTick;

        /// <summary>
        /// The last world tick the server acknowledged for you.
        /// The client should use this to determine the last acked input, as well as to compute
        /// its relative simulation offset.
        /// </summary>
        public uint YourLatestInputTick;

        /// <summary>
        /// States of all players
        /// </summary>
        public PlayerState[] PlayerStates;

        /// <summary>
        /// Contains all player related informations as eg. name, team, etc
        /// </summary>
        public PlayerUpdate[] PlayerUpdates;

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {

            writer.Put(this.WorldTick);
            writer.Put(this.YourLatestInputTick);

            writer.PutArray<PlayerState>(this.PlayerStates);
            writer.PutArray<PlayerUpdate>(this.PlayerUpdates);

        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.WorldTick = reader.GetUInt();
            this.YourLatestInputTick = reader.GetUInt();
            this.PlayerStates = reader.GetArray<PlayerState>();
            this.PlayerUpdates = reader.GetArray<PlayerUpdate>();

        }
    }
}