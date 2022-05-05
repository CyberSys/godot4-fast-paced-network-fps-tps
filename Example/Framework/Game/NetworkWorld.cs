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

using System;
using System.Collections.Generic;
using Godot;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Physics;
using Framework.Utils;
using Framework.Extensions;
using System.Diagnostics;
using Framework.Game.Server;

namespace Framework.Game
{
    /// <summary>
    /// The core world class for server and client worlds
    /// </summary>
    public abstract partial class NetworkWorld : Node3D, INetworkWorld
    {
        /// <summary>
        /// The maximum possible ticks stored and managed by the client and server
        /// </summary>
        public const int MaxTicks = 2048;

        /// <inheritdoc />
        public uint WorldTick { get; protected set; } = 0;

        /// <inheritdoc />
        internal readonly Dictionary<int, NetworkCharacter> _players = new Dictionary<int, NetworkCharacter>();

        /// <inheritdoc />
        public Dictionary<int, NetworkCharacter> Players => _players;

        /// <inheritdoc />
        internal string _resourceWorldPath { get; set; }

        /// <inheritdoc />
        public string ResourceWorldPath => _resourceWorldPath;

        /// <inheritdoc />
        internal string _resourceWorldScriptPath { get; set; }

        /// <inheritdoc />
        public string ResourceWorldScriptPath => _resourceWorldScriptPath;

        internal virtual void PostUpdate() { }

        private InterpolationController interpController = new InterpolationController();

        internal Node playerHolder = new Node();

        /// <summary>
        /// The game logic instance of the world
        /// </summary>
        /// <value></value>
        public GameLogic GameInstance { get; private set; }


        /// <summary>
        /// Adjust the tick rate of the server
        /// </summary>
        /// <returns></returns>
        public ISimulationAdjuster simulationAdjuster { get; set; } = new ServerSimulationAdjuster();

        internal INetworkLevel level;

        /// <inheritdoc />
        public INetworkLevel NetworkLevel => level;

        internal bool isInit = false;

        internal float accumulator;

        internal VarsCollection _serverVars = new VarsCollection();

        /// <inheritdoc />
        public VarsCollection ServerVars => _serverVars;

        /// <inheritdoc />
        public override void _Notification(int what)
        {
            if (what == (int)Godot.Node.NotificationEnterTree)
            {
                this.InternalTreeEntered();
            }
        }

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            this.InternalProcess(delta);
        }

        /// <inheritdoc />
        public override void _PhysicsProcess(float delta)
        {
            this.InternalPhysicsProcess(delta);
        }

        /// <inheritdoc />
        public virtual void OnPlayerInitilaized(INetworkCharacter p)
        {

        }
        /// <inheritdoc />
        internal virtual void OnLevelInternalAddToScene()
        {
            this.OnLevelAddToScene();
        }

        /// <summary>
        /// When level was adding complelty to scene
        /// </summary>
        public virtual void OnLevelAddToScene()
        {

        }

        internal void applyGlow(bool isEnabled)
        {
            var env = this.NetworkLevel?.Environment;
            if (env != null)
                env.GlowEnabled = isEnabled;
        }

        internal void applySDFGI(bool isEnabled)
        {
            var env = this.NetworkLevel?.Environment;
            if (env != null)
                env.SdfgiEnabled = isEnabled;
        }

        internal void applySSAO(bool isEnabled)
        {
            var env = this.NetworkLevel?.Environment;
            if (env != null)
                env.SsaoEnabled = isEnabled;
        }

        internal void applySSIL(bool isEnabled)
        {
            var env = this.NetworkLevel?.Environment;
            if (env != null)
                env.SsilEnabled = isEnabled;
        }

        /// <inheritdoc />
        internal virtual void InternalTreeEntered()
        {
            playerHolder.Name = "playerHolder";
            this.GameInstance = this.GetParent<GameLogic>();

            this.AddChild(playerHolder);
        }


        /// <inheritdoc />
        internal void InstanceLevel(PackedScene res)
        {
            var node = res.Instantiate<NetworkLevel>();
            node.Name = "level";
            this.AddChild(node);

            this.level = node;

            this.OnLevelInternalAddToScene();
        }

        /// <inheritdoc />
        internal virtual void Init(VarsCollection serverVars, uint initalWorldTick)
        {
            this.isInit = true;
            this._serverVars = serverVars;
            foreach (var sv in serverVars.Vars.AllVariables)
            {
                Logger.LogDebug(this, "[Config] " + sv.Key + " => " + sv.Value);
            }
        }

        /// <inheritdoc />
        public virtual void Tick(float interval)
        {
        }


        /// <inheritdoc />
        internal virtual void InternalTick(float interval)
        {
            ++this.WorldTick;
            this.Tick(interval);
        }

        /// <inheritdoc />
        internal virtual void InternalProcess(float delta)
        {
        }

        /// <inheritdoc />
        internal virtual void InternalPhysicsProcess(float delta)
        {
            if (this.isInit == true)
            {
                //physics tick replacement
                var tickInterval = delta;
                accumulator += delta;
                var adjustedTickInterval = tickInterval * simulationAdjuster.AdjustedInterval;
                while (accumulator >= adjustedTickInterval)
                {
                    accumulator -= adjustedTickInterval;
                    interpController.ExplicitFixedUpdate(adjustedTickInterval);

                    InternalTick(tickInterval);
                }

                interpController.ExplicitUpdate(delta);
                PostUpdate();
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            this.QueueFree();
        }
    }
}
