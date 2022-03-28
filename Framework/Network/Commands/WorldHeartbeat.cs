/*
 * Created on Mon Mar 28 2022
 *
 * The MIT License (MIT)
 * Copyright (c) 2022 Stefan Boronczyk, Striked GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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