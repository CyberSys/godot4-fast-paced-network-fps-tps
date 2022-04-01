using System.Linq;
using System.Collections.Generic;
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
using System.Reflection;
using LiteNetLib.Utils;
using System;

namespace Framework.Network.Commands
{


    /// <summary>
    /// The player states structures
    /// Contains all player realted informations eg. position, rotation, velocity
    /// </summary>
    public struct PlayerState : INetSerializable
    {
        /// <summary>
        /// The id of this player
        /// </summary>
        public int Id;

        /// <summary>
        /// Uncomposed list of components and component states
        /// </summary>
        public Dictionary<string, byte[]> NetworkComponents;

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.Id);

            //needs to be at end
            if (this.NetworkComponents != null)
            {
                writer.Put(NetworkComponents.Count);
                foreach (var item in NetworkComponents)
                {
                    writer.Put(item.Key);
                    writer.PutBytesWithLength(item.Value);
                }
            }
            else
            {
                writer.Put(0);
            }
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.Id = reader.GetInt();

            //needs to be at end
            var comps = new Dictionary<string, byte[]>();
            var componentsCount = reader.GetInt();
            for (int i = 0; i < componentsCount; i++)
            {
                string compName = reader.GetString();

                byte[] bytes = reader.GetBytesWithLength();
                comps.Add(compName, bytes);
            }

            this.NetworkComponents = comps;
        }

        /// <inheritdoc />
        public T Decompose<T>(string value) where T : INetSerializable
        {
            if (!this.NetworkComponents.ContainsKey(value))
            {
                return default(T);
            }
            else
            {
                T netcomponent = Activator.CreateInstance<T>();
                var reader = new NetDataReader(this.NetworkComponents[value]);
                netcomponent.Deserialize(reader);

                return netcomponent;
            }
        }
    }
}
