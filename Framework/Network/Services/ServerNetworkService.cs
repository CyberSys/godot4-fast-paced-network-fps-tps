using System;
using System.Collections.Generic;
using Godot;
using LiteNetLib;
using Framework.Network;
using Framework;

namespace Framework.Network.Services
{
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
        public delegate void ClientDisconnectHandler(int clientId, DisconnectReason reason);
        public delegate void ClientLatencyUpdateHandler(int clientId, int latency);
        public delegate void ClientConnectedHandler(int clientId);


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

        /// <summary>
        /// Register the service
        /// </summary>
        public override void Register()
        {
            base.Register();
        }

        /// <summary>
        /// Bind server on specific port
        /// </summary>
        /// <param name="port"></param>
        public void Bind(int port)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            this.netManager = new NetManager(listener);
            this.netManager.BroadcastReceiveEnabled = true;
            this.netManager.AutoRecycle = true;
            this.netManager.EnableStatistics = true;
            this.netManager.UnconnectedMessagesEnabled = true;
            this.netManager.MtuOverride = 8096;

            listener.NetworkErrorEvent += (endPoint, socketErrorCode) =>
            {
                Logger.LogDebug(this, "Issue on " + endPoint.Address + ":" + endPoint.Port + " with error " + socketErrorCode.ToString());
            };

            listener.ConnectionRequestEvent += (request) =>
            {
                ConnectionRequest?.Invoke(request);
            };

            listener.PeerConnectedEvent += peer =>
            {
                Logger.LogDebug(this, peer.Id + " => " + peer.EndPoint.Address + ":" + peer.EndPoint.Port + " conected.");
                ClientConnected?.Invoke(peer.Id);
            };

            listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                _netPacketProcessor.ReadAllPackets(reader, peer);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Logger.LogDebug(this, peer.Id + " => " + peer.EndPoint.Address + ":" + peer.EndPoint.Port + " disconnect with reason " + disconnectInfo.Reason);
                ClientDisconnect?.Invoke(peer.Id, disconnectInfo.Reason);
            };

            listener.NetworkLatencyUpdateEvent += (peer, latency) =>
            {
                PeerLatency[peer] = latency;
                this.ClientLatencyUpdate?.Invoke(peer.Id, latency);
            };

            Logger.LogDebug(this, "Try to start on port " + port);

            var result = this.netManager.Start(port);
            if (result)
            {
                Logger.LogDebug(this, "Bind on port " + port);
                ConnectionEstablished?.Invoke();
            }
        }

        /// <summary>
        /// Unregister the service
        /// </summary>
        public override void Unregister()
        {

            base.Unregister();
            ConnectionShutdown?.Invoke();
        }
    }
}
