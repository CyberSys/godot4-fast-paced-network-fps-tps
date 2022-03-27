using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using Framework.Network.Commands;
using Framework;

namespace Framework.Network
{

    public abstract class NetworkService : IService
    {
        /// <summary>
        /// Default server port
        /// </summary>

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


            this._netPacketProcessor.RegisterNestedType<WorldHeartbeat>();
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
                this._netPacketProcessor.Send<T>(peer, obj, method);
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
