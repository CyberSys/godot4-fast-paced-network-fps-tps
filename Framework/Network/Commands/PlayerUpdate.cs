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
