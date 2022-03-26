using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Shooter.Shared.Network.Packages;

namespace Shooter.Shared
{


    public abstract class NetworkService : IService
    {
        public const int RefreshTime = 10;
        public const string ConnectionKey = "Secure";
        protected readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();
        protected NetManager netManager;
        public Dictionary<NetPeer, int> PeerLatency { get; private set; }
            = new Dictionary<NetPeer, int>();

        public NetworkService()
        {
            this._netPacketProcessor.RegisterNestedType<PlayerInputCommand>();
            this._netPacketProcessor.RegisterNestedType<PlayerState>();
            this._netPacketProcessor.RegisterNestedType<PlayerUpdate>();
            this._netPacketProcessor.RegisterNestedType<ClientInitializer>();
            this._netPacketProcessor.RegisterNestedType<ClientWorldInitializer>();
            this._netPacketProcessor.RegisterNestedType<ServerInitializer>();

            this._netPacketProcessor.RegisterNestedType(
                NetExtensions.SerializeVector3, NetExtensions.DeserializeVector3);

            this._netPacketProcessor.RegisterNestedType(
               NetExtensions.SerializeQuaternion, NetExtensions.DeserializeQuaternion);

            this._netPacketProcessor.RegisterNestedType(
               NetExtensions.SerializeStringDictonary, NetExtensions.DeserializeStringDictonary);
        }


        public void SentMessageToAll<T>(T obj, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            this._netPacketProcessor.Send<T>(this.netManager, obj, method);
        }

        public void SendMessage<T>(int peerId, T obj, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            var peer = this.netManager.GetPeerById(peerId);
            if (peer != null)
            {
                this._netPacketProcessor.Send<T>(peer, obj, method);
            }
        }

        public void SendMessageSerialisable<T>(NetPeer peer, T command, DeliveryMethod method = DeliveryMethod.ReliableOrdered) where T : INetSerializable, new()
        {
            if (this.netManager == null)
            {
                return;
            }

            if (peer != null)
            {
                this._netPacketProcessor.SendNetSerializable(peer, ref command, method);
            }
        }


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

        public void Subscribe<T>(Action<T, NetPeer> onReceive) where T : class, new()
        {
            _netPacketProcessor.SubscribeReusable<T, NetPeer>(onReceive);
        }

        public void SubscribeSerialisable<T>(Action<T, NetPeer> onReceive) where T : INetSerializable, new()
        {
            _netPacketProcessor.SubscribeNetSerializable<T, NetPeer>(onReceive);
        }

        public virtual void Update(float delta)
        {
            netManager?.PollEvents();
        }

        public virtual void Render(float delta)
        {

        }

        public virtual void Register()
        {
        }

        public void Stop()
        {
            netManager.Stop();
            Logger.LogDebug(this, "Shutdown.");
        }

        public virtual void Unregister()
        {
            this.Stop();
        }
    }
}
