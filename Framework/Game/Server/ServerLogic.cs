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
using Framework;
using System.Collections.Generic;
using Framework.Network.Services;
using System;

namespace Framework.Game.Server
{
    /// <summary>
    /// The core server logic
    /// </summary>
    /// <typeparam name="T">The type of the server world</typeparam>
    public abstract class ServerLogic<T> : GameLogic where T : ServerWorld
    {
        /// <inheritdoc />
        internal ServerNetworkService netService = null;

        /// <summary>
        /// maximal possible connections
        /// </summary>
        [Export]
        public int MaxConnections { get; set; } = 15;

        /// <summary>
        /// If clients can connect
        /// </summary>
        [Export]
        public bool AcceptClients { get; set; } = false;

        /// <summary>
        /// The dictonary with all server settings (vars);
        /// </summary>
        [Export]
        public Dictionary<string, string> DefaultVars = new Dictionary<string, string>  {

            // enable or disable interpolation
            { "sv_interpolate", "true" },
            // force for soft or hard lag reduction
            { "sv_agressive_lag_reduction", "true" },
            // maximal input stages per ms
            { "sv_max_stages_ms", "500" },
            // force freeze client in case of lags
            { "sv_freze_client", "false" },

            // crouching
            { "sv_crouching", "true" },
            { "sv_crouching_down_speed", "9.0" },
            { "sv_crouching_up_speed", "4.0" },
            { "sv_crouching_accel", "8.0" },
            { "sv_crouching_deaccel", "4.0" },
            { "sv_crouching_friction", "3.0" },
            { "sv_crouching_speed", "4.0" },

            // walking
            { "sv_walk_accel", "14.0" },
            { "sv_walk_deaccel", "10.0" },

            { "sv_walk_speed", "7.0" },
            { "sv_walk_friction", "6.0" },

            // air control
            { "sv_air_control", "0.4" },
            { "sv_air_accel", "12.0" },
            { "sv_air_deaccel", "2.0" },

            // gravity and jump
            { "sv_gravity", "20.0" },
            { "sv_jumpspeed", "6.5" },

            // side movement
            { "sv_strafe_speed", "1.0" },
            { "sv_strafe_accel", "50.0" },
        };

        public VarsCollection Variables = new VarsCollection(new Vars());

        /// <summary>
        /// Network server port
        /// </summary>
        [Export]
        public int NetworkPort { get; set; } = 27015;

        /// <summary>
        /// Called when after server started succeffull
        /// </summary>
        public virtual void OnServerStarted()
        {

        }

        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();
            this.netService = this.Services.Create<ServerNetworkService>();
            this.netService.ConnectionEstablished += () =>
            {
                Logger.LogDebug(this, "Server was started.");
                this.OnServerStarted();
            };

            this.netService.ConnectionRequest += (request) =>
            {
                if (this.currentWorld == null)
                {
                    request.Reject();
                }
                else
                {
                    if (this.netService.GetConnectionCount() < MaxConnections && AcceptClients && this.OnPreConnect(request))
                    {
                        Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " established");
                        request.AcceptIfKey(this.secureConnectionKey);
                    }
                    else
                    {
                        Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " rejected");
                        request.Reject();
                    }
                }
            };

            base._EnterTree();
            this.netService.Bind(NetworkPort);
        }

        /// <summary>
        /// Called when client connected 
        /// </summary>
        /// <param name="request">boolean for rejected or accepted</param>
        /// <returns></returns>
        public virtual bool OnPreConnect(LiteNetLib.ConnectionRequest request)
        {
            return true;
        }

        /// <inheritdoc />  
        internal override void LoadWorldInternal(string mapName, uint worldTick)
        {
            this.AcceptClients = false;

            //disconnect all clients
            foreach (var connectedClient in this.netService.GetConnectedPeers())
            {
                connectedClient.Disconnect();
            }

            base.LoadWorldInternal(mapName, worldTick);
        }

        /// <inheritdoc />
        internal override void OnMapInstanceInternal(PackedScene res, uint worldTick)
        {
            T newWorld = Activator.CreateInstance<T>();
            newWorld.Name = "world";
            this.AddChild(newWorld);
            newWorld._resourceWorldPath = res.ResourcePath;
            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;

            this.Variables = new VarsCollection(new Vars(this.DefaultVars));
            this.Variables.LoadConfig("server.cfg");

            this.currentWorld?.Init(this.Variables, 0);

            //accept clients
            this.AcceptClients = true;
            this.AfterMapLoaded();
        }

        /// <inheritdoc />
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;

            //reject clients
            this.AcceptClients = false;

            this.AfterMapDestroy();
        }
    }
}
