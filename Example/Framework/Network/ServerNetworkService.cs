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
using Godot;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace Framework.Network
{
    /// <summary>
    /// The server network service
    /// </summary>
    public class ServerNetworkService : NetworkService
    {
        [Signal]
        public event ClientConnectedHandler ClientConnected;

        [Signal]
        public event ClientLatencyUpdateHandler ClientLatencyUpdate;

        [Signal]
        public event ClientDisconnectHandler ClientDisconnect;

        [Signal]
        public event ConnectionEstablishedHandler ConnectionEstablished;

        [Signal]
        public event ConnectionShutdownHandler ConnectionShutdown;

        [Signal]
        public event ConnectionRequestHandler ConnectionRequest;

        public delegate void ConnectionRequestHandler(ConnectionRequest request);
        public delegate void ConnectionShutdownHandler();
        public delegate void ConnectionEstablishedHandler();
        public delegate void ClientDisconnectHandler(short clientId, DisconnectReason reason);
        public delegate void ClientLatencyUpdateHandler(short clientId, int latency);
        public delegate void ClientConnectedHandler(short clientId);

        /// <summary>
        /// Get all connected clients
        /// </summary>
        /// <returns></returns>
        public List<NetPeer> GetConnectedPeers()
        {
            return this.netManager.ConnectedPeerList;
        }

        /// <summary>
        /// Get count of current active connections
        /// </summary>
        /// <returns></returns>
        public int GetConnectionCount()
        {
            return this.netManager.ConnectedPeersCount;
        }

        /// <inheritdoc />
        public override void Register()
        {
            base.Register();
        }

        private EventBasedNetListener listener = null;

        /// <summary>
        /// Bind server on specific port
        /// </summary>
        /// <param name="port"></param>
        public void Bind(int port)
        {
            this.listener = new EventBasedNetListener();
            this.netManager = new NetManager(listener);
            this.netManager.DebugName = "ServerNetworkLayer";
            this.netManager.AutoRecycle = true;
            this.netManager.EnableStatistics = true;
            this.netManager.UnconnectedMessagesEnabled = true;

            listener.NetworkErrorEvent += HandleNetworkError;
            listener.ConnectionRequestEvent += HandleRequest;
            listener.PeerConnectedEvent += HandlePeerConnect;
            listener.NetworkReceiveEvent += HandleReceive;
            listener.PeerDisconnectedEvent += HandlePeerDisconnect;
            listener.NetworkLatencyUpdateEvent += HandleLatency;

            Logger.LogDebug(this, "Try to start on port " + port);

            var result = this.netManager.Start(port);
            if (result)
            {
                Logger.LogDebug(this, "Bind on port " + port);
                ConnectionEstablished?.Invoke();
            }
        }

        private void HandleRequest(LiteNetLib.ConnectionRequest request)
        {
            ConnectionRequest?.Invoke(request);
        }

        private void HandleNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            Logger.LogDebug(this, "Issue on " + endPoint.Address + ":" + endPoint.Port + " with error " + socketErrorCode.ToString());
        }

        private void HandleLatency(NetPeer peer, int latency)
        {
            PeerLatency[peer] = latency;
            this.ClientLatencyUpdate?.Invoke((short)peer.Id, latency);
        }

        private void HandlePeerDisconnect(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.LogDebug(this, peer.Id + " => " + peer.EndPoint.Address + ":" + peer.EndPoint.Port + " disconnect with reason " + disconnectInfo.Reason);
            ClientDisconnect?.Invoke((short)peer.Id, disconnectInfo.Reason);
        }

        private void HandleReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            _netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void HandlePeerConnect(NetPeer peer)
        {
            Logger.LogDebug(this, peer.Id + " => " + peer.EndPoint.Address + ":" + peer.EndPoint.Port + " conected.");
            ClientConnected?.Invoke((short)peer.Id);
        }

        /// <inheritdoc />
        public override void Unregister()
        {
            listener.NetworkErrorEvent += HandleNetworkError;
            listener.ConnectionRequestEvent += HandleRequest;
            listener.PeerConnectedEvent += HandlePeerConnect;
            listener.NetworkReceiveEvent += HandleReceive;
            listener.PeerDisconnectedEvent += HandlePeerDisconnect;
            listener.NetworkLatencyUpdateEvent += HandleLatency;

            base.Unregister();
            ConnectionShutdown?.Invoke();
        }
    }
}
