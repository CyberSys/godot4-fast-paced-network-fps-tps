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
using Framework.Game;
using Framework.Game.Server;
using Framework;
using Framework.Network.Commands;
using LiteNetLib;
using Framework.Network.Services;
using System;

using System.Collections.Generic;

namespace Framework.Game.Client
{
    /// <summary>
    /// Is the base class for any client (SubViewport)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientLogic<T> : GameLogic where T : ClientWorld
    {

        /// <summary>
        /// The dictonary with all server settings (vars);
        /// </summary>
        [Export]
        public Dictionary<string, string> DefaultVars = new Dictionary<string, string>
        {
           { "cl_sensitivity", "2.0"}
        };

        /// <inheritdoc />
        private ClientNetworkService netService = null;

        /// <inheritdoc />
        private string loadedWorldName = null;

        /// <inheritdoc />
        internal override void OnMapInstanceInternal(PackedScene res, uint worldTick)
        {
            T newWorld = Activator.CreateInstance<T>();
            newWorld.Name = "world";

            this.AddChild(newWorld);

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;

            //send server map loading was completed
            this.netService.SendMessageSerialisable<ServerInitializer>(0, new ServerInitializer());
            this.AfterMapLoaded();
        }

        /// <inheritdoc />        
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.AfterMapDestroy();
        }

        /// <summary>
        /// Disconnect the client
        /// </summary>
        public void Disconnect()
        {
            this.DestroyMapInternal();
            this.netService.Disconnect();
        }

        /// <summary>
        /// Connect with an server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            if (this.CurrentWorld == null)
            {
                this.netService.Connect(new ClientConnectionSettings
                {
                    Port = port,
                    Hostname = hostname,
                    SecureKey = this.secureConnectionKey
                });
            }
        }

        /// <summary>
        /// On local client is connected
        /// </summary>
        public virtual void OnConnected()
        {

        }

        /// <summary>
        /// On local client are disconneted
        /// </summary>
        public virtual void OnDisconnect()
        {

        }

        /// <inheritdoc />  
        internal override void InternalTreeEntered()
        {
            ClientSettings.Variables = new VarsCollection(new Vars(this.DefaultVars));
            ClientSettings.Variables.LoadConfig("client.cfg");

            this.netService = this.Services.Create<ClientNetworkService>();
            this.netService.OnDisconnect += this.OnInternalDisconnect;
            this.netService.Connected += this.OnConnected;

            this.netService.SubscribeSerialisable<ClientWorldInitializer>((package, peer) =>
            {
                if (this.loadedWorldName != package.WorldName)
                {
                    this.loadedWorldName = package.WorldName;
                    this.LoadWorldInternal(package.WorldName, package.WorldTick);
                }
            });


            base.InternalTreeEntered();
        }

        /// <inheritdoc />  
        private void OnInternalDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.DestroyMapInternal();
                this.OnDisconnect();
            }
        }
    }
}
