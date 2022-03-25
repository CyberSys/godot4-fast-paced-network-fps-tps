using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using LiteNetLib;
using Shooter.Shared;
using Shooter.Shared.Network.Packages;

namespace Shooter.Client.Services
{
    public class ClientNetworkService : NetworkService
    {
        [Signal]
        public event ConnectedHandler Connected;
        public delegate void ConnectedHandler();

        [Signal]
        public event DisconnectHandler OnDisconnect;
        public delegate void DisconnectHandler(DisconnectReason reason, bool fullDisconnect);

        public const int Port = 27015;
        public const int MaxRetries = 10;
        public const int RetryDelay = 2;
        public int currentRetries = 0;
        public bool canRetry = false;
        public float RetryTime = 0;
        private NetPeer currentPeer;

        private string lastHostname = null;

        private NetPeer serverPeer;

        public NetPeer ServerPeer => serverPeer;

        public NetStatistics Statistics => this.netManager.Statistics;

        private long _bytesSended = 0;
        private long _packageLoss = 0;

        private long _bytesReceived = 0;
        private long _packageLossPercent = 0;

        public long bytesSended => _bytesSended;
        public long packageLoss => _packageLoss;

        public long bytesReceived => _bytesReceived;
        public long packageLossPercent => _packageLossPercent;
        public int ping => _ping;

        public int _ping = 0;

        public int MyId = -1;

        public void Disconnect()
        {
            this.currentRetries = 0;
            this.RetryTime = 0;
            this.canRetry = false;
            this.currentPeer?.Disconnect();
        }

        public override void Register()
        {
            base.Register();
            this.Disconnect();

            this.Start();
        }

        private float nextStaticsUpdate = 0f;

        public override void Update(float delta)
        {
            if (this.canRetry && this.lastHostname != null)
            {
                if (RetryTime <= 0)
                {
                    this.Connect(this.lastHostname);
                    RetryTime += RetryDelay;
                }
                else if (RetryTime >= 0)
                {
                    RetryTime -= delta;
                }
            }

            if (nextStaticsUpdate >= 1f)
            {
                nextStaticsUpdate = 0f;
                this._bytesSended = this.Statistics.BytesSent;
                this._bytesReceived = this.Statistics.BytesSent;
                this._packageLoss = this.Statistics.PacketLoss;
                this._packageLossPercent = this.Statistics.PacketLossPercent;

                this.Statistics.Reset();
            }
            else
            {
                nextStaticsUpdate += delta;
            }

            base.Update(delta);
        }

        public void Start()
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            this.netManager = new NetManager(listener);
            this.netManager.BroadcastReceiveEnabled = true;
            this.netManager.AutoRecycle = true;
            this.netManager.EnableStatistics = true;
            this.netManager.UnconnectedMessagesEnabled = true;
            this.netManager.MtuOverride = 8096;


            listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                //after last receive increase max retries
                this.currentRetries = MaxRetries;
                _netPacketProcessor.ReadAllPackets(reader, peer);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Logger.LogDebug(this, "Disconnected with reason " + disconnectInfo.Reason);

                currentRetries++;
                if (currentRetries <= MaxRetries && disconnectInfo.Reason == DisconnectReason.RemoteConnectionClose)
                {
                    this.canRetry = true;
                    this.OnDisconnect?.Invoke(disconnectInfo.Reason, false);
                }
                else
                {
                    this.canRetry = false;
                    this.OnDisconnect?.Invoke(disconnectInfo.Reason, true);
                }
            };

            listener.PeerConnectedEvent += (peer) =>
            {
                this.serverPeer = peer;
            };

            listener.NetworkLatencyUpdateEvent += (peer, latency) =>
            {
                if (peer == this.ServerPeer)
                {
                    _ping = latency;
                }

                PeerLatency[peer] = latency;
            };

            this.netManager.Start();
        }

        public void Connect(string Hostname)
        {
            var task = new System.Threading.Tasks.Task(() =>
            {
                this.canRetry = false;
                this.lastHostname = Hostname;
                Logger.LogDebug(this, "Try to start to tcp://" + Hostname + ":" + Port);
                this.currentPeer = this.netManager.Connect(Hostname, Port, ConnectionKey);
                if (this.currentPeer != null)
                {
                    Logger.LogDebug(this, "Connected with remote id " + this.currentPeer.RemoteId);
                    Connected?.Invoke();
                }
            });

            task.Start();
        }

        public override void Unregister()
        {
            base.Unregister();
        }
    }
}
