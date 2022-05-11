
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
using LiteNetLib;
using Framework.Network;
using Framework;

namespace Framework.Network
{
    /// <summary>
    /// Settings for the client connection
    /// </summary>
    public struct ClientConnectionSettings
    {
        /// <summary>
        /// Server port
        /// </summary>
        /// <value></value>
        public int Port { get; set; }

        /// <summary>
        /// Server hostname
        /// </summary>
        /// <value></value>
        public string Hostname { get; set; }

        /// <summary>
        /// The server connect password
        /// </summary>
        /// <value></value>
        public string SecureKey { get; set; }
    }

    /// <summary>
    /// The client network service
    /// </summary>
    public class ClientNetworkService : NetworkService
    {
        private int currentRetries = 0;
        private float nextStaticsUpdate = 0f;
        private bool canRetry = false;
        private float RetryTime = 0;

        private long _bytesSended = 0;
        private long _packageLoss = 0;
        private long _bytesReceived = 0;
        private long _packageLossPercent = 0;

        /// <summary>
        /// Triggered when the client is connected.
        /// </summary>
        [Signal]
        public event ConnectedHandler Connected;

        /// <summary>
        /// Connection handler for connect event
        /// </summary>
        public delegate void ConnectedHandler();

        /// <summary>
        /// Triggered when the client is disconnected
        /// </summary>
        [Signal]
        public event DisconnectHandler OnDisconnect;

        /// <summary>
        /// Disconnect handler for disconnect event
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="fullDisconnect"></param>
        public delegate void DisconnectHandler(DisconnectReason reason, bool fullDisconnect);

        /// <summary>
        /// Maximum of retries
        /// </summary>
        [Export]
        public int MaxRetriesPerConnection = 10;

        /// <summary>
        /// Delay between reconnect
        /// </summary>
        [Export]
        public int ConnectionRetryDelay = 2;

        private NetPeer currentPeer;

        private ClientConnectionSettings lastConnectionSettings;

        private NetPeer serverPeer;

        /// <summary>
        /// The network server peer
        /// </summary>
        public NetPeer ServerPeer => serverPeer;

        private NetStatistics Statistics => this.netManager.Statistics;

        /// <summary>
        /// Sended bytes since last 1 second
        /// </summary>
        public long BytesSended => _bytesSended;

        /// <summary>
        /// Lossing packages since last 1 second
        /// </summary>
        public long PackageLoss => _packageLoss;

        /// <summary>
        /// Received bytes since last 1 second
        /// </summary>
        public long BytesReceived => _bytesReceived;

        /// <summary>
        /// Package loose in percent (avg) since last 1 second
        /// </summary>
        public long PackageLossPercent => _packageLossPercent;
        private int _ping = 0;

        /// <summary>
        /// Get the current latency between server and client
        /// </summary>
        public int Ping => _ping;

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            this.currentRetries = 0;
            this.RetryTime = 0;
            this.canRetry = false;
            this.currentPeer?.Disconnect();
        }
        private EventBasedNetListener listener = null;
        /// <inheritdoc />
        public override void Register()
        {
            base.Register();
            this.Disconnect();

            this.listener = new EventBasedNetListener();

            this.netManager = new NetManager(listener);
            this.netManager.DebugName = "ClientNetworkLayer";
            this.netManager.AutoRecycle = true;
            this.netManager.EnableStatistics = true;
            this.netManager.UnconnectedMessagesEnabled = true;

            listener.NetworkReceiveEvent += HandleReceive;
            listener.PeerDisconnectedEvent += HandlePeerDisconnect;
            listener.PeerConnectedEvent += HandlePeerConnect;
            listener.NetworkLatencyUpdateEvent += HandleLatency;

            this.netManager.Start();
        }
        private void HandleReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            //after last receive increase max retries
            this.currentRetries = MaxRetriesPerConnection;
            _netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void HandlePeerConnect(NetPeer peer)
        {
            this.serverPeer = peer;
        }
        private void HandlePeerDisconnect(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.LogDebug(this, "Disconnected with reason " + disconnectInfo.Reason);

            currentRetries++;
            if (currentRetries <= MaxRetriesPerConnection && disconnectInfo.Reason == DisconnectReason.RemoteConnectionClose)
            {
                this.canRetry = true;
                this.OnDisconnect?.Invoke(disconnectInfo.Reason, false);
            }
            else
            {
                this.canRetry = false;
                this.OnDisconnect?.Invoke(disconnectInfo.Reason, true);
            }
        }

        private void HandleLatency(NetPeer peer, int latency)
        {
            if (peer == this.ServerPeer)
            {
                _ping = latency;
            }

            PeerLatency[peer] = latency;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            if (this.canRetry && !this.lastConnectionSettings.Equals(default(ClientConnectionSettings)))
            {
                if (RetryTime <= 0)
                {
                    this.Connect(this.lastConnectionSettings);
                    RetryTime += ConnectionRetryDelay;
                }
                else if (RetryTime >= 0)
                {
                    RetryTime -= delta;
                }
            }

            base.Update(delta);

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
        }

        /// <summary>
        /// Connect to an server
        /// </summary>
        /// <param name="settings">The connection settings eg. hostname, port, secure</param>
        public void Connect(ClientConnectionSettings settings)
        {
            var task = new System.Threading.Tasks.Task(() =>
            {
                this.canRetry = false;
                this.lastConnectionSettings = settings;
                Logger.LogDebug(this, "Try to start to tcp://" + settings.Hostname + ":" + settings.Port);
                this.currentPeer = this.netManager.Connect(settings.Hostname, settings.Port, settings.SecureKey);
                if (this.currentPeer != null)
                {
                    Logger.LogDebug(this, "Receive server handshake");
                    Connected?.Invoke();
                }
            });

            task.Start();
        }

        /// <inheritdoc />
        public override void Unregister()
        {
            listener.NetworkReceiveEvent -= HandleReceive;
            listener.PeerDisconnectedEvent -= HandlePeerDisconnect;
            listener.PeerConnectedEvent -= HandlePeerConnect;
            listener.NetworkLatencyUpdateEvent -= HandleLatency;

            base.Unregister();
        }
    }
}
