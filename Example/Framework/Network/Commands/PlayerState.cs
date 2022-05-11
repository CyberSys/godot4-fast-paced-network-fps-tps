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
using System.Linq;
using System.Collections.Generic;
using Framework.Game;

namespace Framework.Network.Commands
{
    public struct PlayerNetworkVarState : INetSerializable
    {
        public short Key;
        public byte[] Data;

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Key);
            writer.PutBytesWithLength(Data);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.Key = reader.GetShort();
            this.Data = reader.GetBytesWithLength();
        }
    }


    public enum PlayerStateVarType
    {
        Permanent,
        Sync
    }

    /// <summary>
    /// The player states structures
    /// Contains all player realted informations eg. position, rotation, velocity
    /// </summary>
    public struct PlayerState : INetSerializable
    {
        /// <summary>
        /// Current latency
        /// </summary>
        public short Latency;

        /// <summary>
        /// The id of this player
        /// </summary>
        public short NetworkId;

        /// <summary>
        /// Uncomposed list of components and component states
        /// </summary>
        public List<PlayerNetworkVarState> NetworkSyncedVars;


        internal byte[] ParseAnyTypeToBytes(object value)
        {
            if (value == null)
            {
                throw new Exception("Cant be null");
            }

            var writer = new NetDataWriter();

            if (value.GetType().IsEnum)
                writer.Put((uint)value);
            else if (value is float)
                writer.Put((float)value);
            else if (value is string)
                writer.Put((string)value);
            else if (value is int)
                writer.Put((int)value);
            else if (value is bool)
                writer.Put((bool)value);
            else if (value is short)
                writer.Put((short)value);
            else if (value is uint)
                writer.Put((uint)value);
            else if (value is Quaternion)
                writer.Put((Quaternion)value);
            else if (value is Vector3)
                writer.Put((Vector3)value);
            else if (value is Vector2)
                writer.Put((Vector2)value);
            else
                throw new Exception(value.GetType() + " unknown parse type.");

            var bytes = writer.CopyData();
            return bytes;
        }

        internal object ParseBytesToAnyType(Type t, byte[] value)
        {
            var reader = new NetDataReader(value);
            if (t.IsEnum)
            {
                return Enum.ToObject(t, reader.GetUInt());
            }
            else if (t == typeof(int))
            {
                return reader.GetInt();
            }
            else if (t == typeof(float))
            {
                return reader.GetFloat();
            }
            else if (t == typeof(string))
            {
                return reader.GetString();
            }
            else if (t == typeof(Vector3))
            {
                return reader.GetVector3();
            }
            else if (t == typeof(Vector2))
            {
                return reader.GetVector2();
            }
            else if (t == typeof(bool))
            {
                return reader.GetBool();
            }
            else if (t == typeof(short))
            {
                return reader.GetShort();
            }
            else if (t == typeof(uint))
            {
                return reader.GetUInt();
            }
            else if (t == typeof(Godot.Quaternion))
            {
                return reader.GetQuaternion();
            }
            else
            {
                return null;
            }
        }

        public object GetVar(INetworkObject netObject, string key)
        {
            var checkCollection = new Dictionary<string, NetworkAttribute>();

            var collection = this.NetworkSyncedVars != null ? this.NetworkSyncedVars : new List<PlayerNetworkVarState>();
            checkCollection = netObject.NetworkSyncVars;

            if (collection == null || checkCollection == null || netObject == null)
            {
                return null;
            }

            if (!checkCollection.ContainsKey(key))
            {
                Logger.LogDebug(this, "Key " + key + " not registered.");
                return null;
            }

            var origNetVar = checkCollection[key];
            if (collection.Count(df => df.Key == origNetVar.AttributeIndex) <= 0)
            {
                Logger.LogDebug(this, "Key " + key + " not in network package.");
                return null;
            }

            PlayerNetworkVarState value = collection.First(df => df.Key == origNetVar.AttributeIndex);
            if (value.Data == null)
            {
                return null;
            }

            var result = this.ParseBytesToAnyType(origNetVar.AttributeType, value.Data);
            if (result == null)
            {
                throw new Exception("Result cant be null for " + key + "from type" + origNetVar.AttributeType + " with length of " + value.Data.Length);
            }

            return result;
        }

        public T GetVar<T>(INetworkObject netObject, string key, T fallback = default(T))
        {
            if (netObject == null)
                return fallback;

            var var = this.GetVar(netObject, key);
            if (var == null)
            {
                return fallback;
            }
            else if (var is T)
            {
                return (T)var;
            }
            else
            {
                return fallback;
            }
        }

        public void SetVar(INetworkObject netObject, string key, object value)
        {
            if (value == null)
            {
                throw new Exception("Cant be null);");
            }

            var collection = this.NetworkSyncedVars != null ? this.NetworkSyncedVars : new List<PlayerNetworkVarState>();
            var checkCollection = new Dictionary<string, NetworkAttribute>();

            collection = this.NetworkSyncedVars;
            checkCollection = netObject.NetworkSyncVars;

            if (!checkCollection.ContainsKey(key))
            {
                throw new Exception("PermanentNetworkVar with  " + key + " or from type  " + value.GetType() + " not found.");
            }
            else
            {
                var orig = checkCollection[key];
                var bytes = this.ParseAnyTypeToBytes(value);
                if (bytes.Length > 0)
                {
                    var newValue = new PlayerNetworkVarState { Key = orig.AttributeIndex, Data = bytes };
                    var findIndex = collection.FindIndex(df => df.Key == orig.AttributeIndex);
                    if (findIndex == -1)
                    {
                        collection.Add(newValue);
                    }
                    else
                    {
                        collection[findIndex] = newValue;
                    }
                }
                else
                {
                    throw new Exception("Empty byte array??");
                }
            }
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NetworkId);
            writer.Put(Latency);

            if (NetworkSyncedVars == null)
                NetworkSyncedVars = new List<PlayerNetworkVarState>();

            writer.PutArray<PlayerNetworkVarState>(NetworkSyncedVars.ToArray());
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.NetworkId = reader.GetShort();
            this.Latency = reader.GetShort();

            this.NetworkSyncedVars = reader.GetArray<PlayerNetworkVarState>().ToList();
        }
    }
}
