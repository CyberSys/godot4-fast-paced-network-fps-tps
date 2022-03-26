
using Godot;
using LiteNetLib;
using Framework.Network;
using Framework;

namespace Framework.Network.Services
{
    public struct ClientConnectionSettings
    {
        public int Port { get; set; }
        public string Hostname { get; set; }
        public string SecureKey { get; set; }
    }

    public class ClientNetworkService : NetworkService
    {
        [Signal]
        public event ConnectedHandler Connected;
        public delegate void ConnectedHandler();

        [Signal]
        public event DisconnectHandler OnDisconnect;
        public delegate void DisconnectHandler(DisconnectReason reason, bool fullDisconnect);

        [Export]
        public int MaxRetriesPerConnection = 10;

        [Export]
        public const int ConnectionRetryDelay = 2;

        private int currentRetries = 0;
        private float nextStaticsUpdate = 0f;
        private bool canRetry = false;
        private float RetryTime = 0;

        private NetPeer currentPeer;

        private ClientConnectionSettings lastConnectionSettings;

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
        private int _ping = 0;
        public int ping => _ping;

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
                this.currentRetries = MaxRetriesPerConnection;
                _netPacketProcessor.ReadAllPackets(reader, peer);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
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
