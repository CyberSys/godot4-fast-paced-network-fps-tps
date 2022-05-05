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

using LiteNetLib.Utils;
namespace Framework.Network.Commands
{
    /// <summary>
    /// Contains all player relevant fields eg Latency, Team, Required Components
    /// </summary>
    public struct PlayerUpdate : INetSerializable
    {
        /// <summary>
        /// Current network connection state
        /// </summary>
        public PlayerConnectionState State;

        /// <summary>
        /// Current player name
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// Current network id
        /// </summary>
        public int Id;

        /// <summary>
        /// Current latency
        /// </summary>
        public int Latency;

        /// <summary>
        /// Required local player components
        /// </summary>
        public int[] RequiredComponents;

        /// <summary>
        /// Required puppet components
        /// </summary>
        public int[] RequiredPuppetComponents;

        /// <summary>
        /// Time since player is disconnected
        /// </summary>
        public float DisconnectTime;

        /// <summary>
        /// Tick of latest input
        /// </summary>
        public uint LatestInputTick;

        /// <summary>
        /// Resource path to the scene
        /// </summary>
        public string ResourcePath;

        /// <summary>
        /// Script path of the scene
        /// </summary>
        public string ScriptPath;

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {

            writer.Put(this.Id);
            writer.Put((int)this.State);
            writer.Put(this.PlayerName);
            writer.Put(this.Latency);
            writer.Put(this.ResourcePath);
            writer.Put(this.ScriptPath);
            writer.PutArray(this.RequiredComponents);
            writer.PutArray(this.RequiredPuppetComponents);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.Id = reader.GetInt();
            this.State = (PlayerConnectionState)reader.GetInt();
            this.PlayerName = reader.GetString();
            this.Latency = reader.GetInt();
            this.ResourcePath = reader.GetString();
            this.ScriptPath = reader.GetString();

            this.RequiredComponents = reader.GetIntArray();
            this.RequiredPuppetComponents = reader.GetIntArray();
        }
    }
}
