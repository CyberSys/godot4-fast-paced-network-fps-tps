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
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Reflection;
using Framework.Network;
using System;

namespace Framework.Game
{
    /// <summary>
    /// The game object network mode
    /// </summary>
    public enum NetworkMode
    {
        /// <summary>
        /// Game object is an server object
        /// </summary>
        SERVER,
        /// <summary>
        /// Game object is an local object
        /// </summary>
        CLIENT,
        /// <summary>
        /// Game object is an local object as puppet
        /// </summary>
        PUPPET
    }
    /// <summary>
    /// Required interface for network objects
    /// </summary>
    public interface INetworkObject : IBaseComponent
    {
        /// <summary>
        /// Id of game object
        /// </summary>
        /// <value></value>
        public short NetworkId { get; set; }

        /// <summary>
        /// The network mode for the game object
        /// </summary>
        /// <value></value>
        public NetworkMode Mode { get; set; }

        /// <summary>
        /// The script path (mono) of the component
        /// </summary>
        /// <value></value>
        public string[] ScriptPaths { get; set; }

        /// <summary>
        /// The resource path of the component
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; }

        public object Get(string property);

        public void Set(string property, object value);

        public Dictionary<string, NetworkAttribute> NetworkSyncVars { get; }
    }

    /// <summary>
    /// Network attribute
    /// </summary>
    public struct NetworkAttribute
    {
        public short AttributeIndex { get; set; }
        public System.Type AttributeType { get; set; }

        public NetworkSyncFrom From { get; set; }
        public NetworkSyncTo To { get; set; }
    }

    /// <inheritdoc />
    public static class INetworkObjectExtension
    {
        /// <inheritdoc />
        public static bool IsServer(this INetworkObject client)
        {
            return client.Mode == NetworkMode.SERVER;
        }

        /// <inheritdoc />
        public static bool IsLocal(this INetworkObject client)
        {
            return client.Mode == NetworkMode.CLIENT;
        }

        /// <inheritdoc />
        public static bool IsPuppet(this INetworkObject client)
        {
            return client.Mode == NetworkMode.PUPPET;
        }

        /// <summary>
        /// Get the player network attributes
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, NetworkAttribute> GetNetworkAttributes(this INetworkObject networkObject)
        {
            var list = new Dictionary<string, NetworkAttribute>();
            short i = 0;
            foreach (var prop in networkObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(prop, typeof(NetworkVar)))
                {
                    NetworkVar customattribute = (NetworkVar)prop.GetCustomAttribute(typeof(NetworkVar), false);

                    list.Add(prop.Name, new NetworkAttribute { AttributeType = prop.FieldType, AttributeIndex = i, From = customattribute.From, To = customattribute.To });
                    i++;
                }
            }

            foreach (var prop in networkObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(prop, typeof(NetworkVar)))
                {
                    list.Add(prop.Name, new NetworkAttribute { AttributeType = prop.GetType(), AttributeIndex = i });
                    i++;
                }
            }

            return list;
        }
    }
}
