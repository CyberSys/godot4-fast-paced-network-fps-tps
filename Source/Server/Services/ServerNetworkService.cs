using System;
using System.Collections.Generic;
using Godot;
using LiteNetLib;
using Shooter.Shared;
namespace Shooter.Server.Services
{
    public class ServerNetworkService : NetworkService
    {
        [Signal]
        public event ClientConnectedHandler ClientConnected;
        public delegate void ClientConnectedHandler(int clientId);
        [Signal]
        public event ClientLatencyUpdateHandler ClientLatencyUpdate;
        public delegate void ClientLatencyUpdateHandler(int clientId, int latency);

        [Signal]
        public event ClientDisconnectHandler ClientDisconnect;
        public delegate void ClientDisconnectHandler(int clientId, DisconnectReason reason);

        [Signal]
        public event ConnectionEstablishedHandler ConnectionEstablished;
        public delegate void ConnectionEstablishedHandler();

        [Signal]
        public event ConnectionShutdownHandler ConnectionShutdown;
        public delegate void ConnectionShutdownHandler();

        public const int Port = 27015;
        public int MaxConnections = 15;
        public bool acceptClients = false;


        public List<NetPeer> GetConnectedPeers()
        {
            return this.netManager.ConnectedPeerList;
        }

        public override void Register()
        {

            base.Register();

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
                if (this.netManager.ConnectedPeersCount < MaxConnections && acceptClients)
                {
                    Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " established");
                    request.AcceptIfKey(ConnectionKey);
                }
                else
                {
                    Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " rejected");
                    request.Reject();
                }
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

            Logger.LogDebug(this, "Try to start on port " + Port);

            var result = this.netManager.Start(Port);
            if (result)
            {
                Logger.LogDebug(this, "Bind on port " + Port);
                ConnectionEstablished?.Invoke();
            }
        }

        public override void Unregister()
        {

            base.Unregister();
            ConnectionShutdown?.Invoke();
        }
    }
}
