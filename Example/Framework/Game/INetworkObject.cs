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

using Framework.Network;
using System.Collections.Generic;

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
        /// The network mode for the game object
        /// </summary>
        /// <value></value>
        public NetworkMode Mode { get; set; }

        /// <summary>
        /// The script path (mono) of the component
        /// </summary>
        /// <value></value>
        public string ScriptPath { get; set; }

        /// <summary>
        /// The resource path of the component
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; }
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
    }
}
