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
using Framework.Game;
using System.Collections.Generic;

namespace Framework.Network.Commands
{
    /// <summary>
    /// Network command for an client, after map was loaded sucessfull
    /// Contains all server relevated settings and vars
    /// </summary>
    public struct ClientWorldInitializer : INetSerializable
    {
        /// <summary>
        /// Current server world tick
        /// </summary>
        public uint GameTick;

        /// <summary>
        /// Own player id on server
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// Server variables
        /// </summary>
        public Vars ServerVars;


        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.GameTick);
            writer.Put(this.PlayerId);
            this.ServerVars.Serialize(writer);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.GameTick = reader.GetUInt();
            this.PlayerId = reader.GetInt();
            this.ServerVars.Deserialize(reader);
        }
    }
}
