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

using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;

namespace Framework.Network
{

    /// <summary>
    /// Base network service class
    /// </summary>
    public abstract class NetworkService : IService
    {
        internal readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        internal NetManager netManager;

        /// <summary>
        /// Latency of each player
        /// </summary>
        /// <value></value>
        public Dictionary<NetPeer, int> PeerLatency { get; private set; }
            = new Dictionary<NetPeer, int>();

        /// <inheritdoc />
        public NetworkService()
        {
            this._netPacketProcessor.RegisterNestedType<PlayerInput>();
            this._netPacketProcessor.RegisterNestedType<PlayerState>();
            this._netPacketProcessor.RegisterNestedType<PlayerInfo>();
            this._netPacketProcessor.RegisterNestedType<Input.GeneralPlayerInput>();
            this._netPacketProcessor.RegisterNestedType<Framework.Game.Vars>();
            this._netPacketProcessor.RegisterNestedType<ClientWorldLoader>();
            this._netPacketProcessor.RegisterNestedType<ServerInitializer>();
            this._netPacketProcessor.RegisterNestedType<RaycastTest>();
            this._netPacketProcessor.RegisterNestedType<PlayerNetworkVarState>();

            this._netPacketProcessor.RegisterNestedType(
                NetExtensions.SerializeVector3, NetExtensions.DeserializeVector3);

            this._netPacketProcessor.RegisterNestedType(
               NetExtensions.SerializeQuaternion, NetExtensions.DeserializeQuaternion);

            this._netPacketProcessor.RegisterNestedType(
               NetExtensions.SerializeStringDictonary, NetExtensions.DeserializeStringDictonary);
        }

        /// <summary>
        /// Send an network message to all clients
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SentMessageToAll<T>(T obj, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            this._netPacketProcessor.Send<T>(this.netManager, obj, method);
        }

        /// <summary>
        /// Send an network message to all clients
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SentMessageToAllSerialized<T>(T obj, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : INetSerializable, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            this._netPacketProcessor.SendNetSerializable<T>(this.netManager, ref obj, method);
        }

        /// <summary>
        /// Send an message to an specific client (non serialized)
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <typeparam name="T"></typeparam>
        public void SendMessage<T>(int peerId, T obj, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            var peer = this.netManager.GetPeerById(peerId);
            if (peer != null)
            {
                this._netPacketProcessor.Send(peer, obj, method);
            }
        }

        /// <summary>
        /// Send an network message to an specific client by id (serialized)
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="command"></param>
        /// <param name="method"></param>
        /// <typeparam name="T"></typeparam>
        public void SendMessageSerialisable<T>(int peerId, T command, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : INetSerializable, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            var peer = this.netManager.GetPeerById(peerId);
            if (peer != null)
            {
                this._netPacketProcessor.SendNetSerializable(peer, ref command, method);
            }
        }

        /// <summary>
        /// Subscribe an network command from type class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>(Action<T, NetPeer> onReceive) where T : class, new()
        {
            _netPacketProcessor.SubscribeReusable<T, NetPeer>(onReceive);
        }

        /// <summary>
        /// Subscribe an network command from type INetSerializable
        /// </summary>
        /// <param name="onReceive"></param>
        /// <typeparam name="T"></typeparam>
        public void SubscribeSerialisable<T>(Action<T, NetPeer> onReceive) where T : INetSerializable, new()
        {
            _netPacketProcessor.SubscribeNetSerializable<T, NetPeer>(onReceive);
        }

        /// <inheritdoc />
        public virtual void Update(float delta)
        {
            netManager?.PollEvents();
        }

        /// <inheritdoc />
        public virtual void Render(float delta)
        {

        }

        /// <inheritdoc />
        public virtual void Register()
        {
        }

        /// <inheritdoc />
        private void Stop()
        {
            netManager.Stop();
            Logger.LogDebug(this, "Shutdown.");
        }

        /// <inheritdoc />
        public virtual void Unregister()
        {
            this.Stop();
        }
    }
}
