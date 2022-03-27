using System;
using System.Collections.Generic;
using Godot;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Physics;
using Framework.Utils;
using Framework.Extensions;

namespace Framework.Game
{
    public interface ISimulationAdjuster
    {
        float AdjustedInterval { get; }
    }

    public class NoopAdjuster : ISimulationAdjuster
    {
        public float AdjustedInterval { get; } = 1.0f;
    }

    /// <summary>
    /// The core world class for server and client worlds
    /// </summary>
    public abstract partial class World : Node3D, IWorld
    {
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
        public virtual void OnPlayerInitilaized(IPlayer p)
        {

        }

        /// <inheritdoc />
        internal virtual void InternalTreeEntered()
        {
            playerHolder.Name = "playerHolder";
            this.gameInstance = this.GetParent<GameLogic>();

            this.AddChild(playerHolder);
        }

        /// <inheritdoc />
        public uint WorldTick { get; protected set; } = 0;

        /// <inheritdoc />
        internal readonly Dictionary<int, IPlayer> _players = new Dictionary<int, IPlayer>();

        /// <inheritdoc />
        public Dictionary<int, IPlayer> Players => _players;

        /// <inheritdoc />
        internal string _resourceWorldPath { get; set; }

        /// <inheritdoc />
        public string ResourceWorldPath => _resourceWorldPath;

        internal virtual void PostUpdate() { }

        private InterpolationController interpController = new InterpolationController();

        protected Node playerHolder = new Node();
        protected AsyncLoader loader = new AsyncLoader();
        protected GameLogic gameInstance = null;
        private float accumulator;
        protected float adjustedInterval = 1.0f;
        protected ISimulationAdjuster simulationAdjuster = new NoopAdjuster();

        protected ILevel level;
        public ILevel Level => level;

        protected bool isInit = false;

        protected Dictionary<string, string> _serverVars = new Dictionary<string, string>();
        public Dictionary<string, string> ServerVars => _serverVars;


        /// <inheritdoc />
        internal void InstanceLevel(PackedScene res)
        {
            var node = res.Instantiate<Level>();
            node.Name = "level";
            this.AddChild(node);

            this.level = node;
        }

        /// <inheritdoc />
        public virtual void Init(Dictionary<string, string> serverVars, uint initalWorldTick)
        {
            this.isInit = true;
            this._serverVars = serverVars;
            foreach (var sv in serverVars)
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
            //async loader
            this.loader.Tick();
        }

        /// <inheritdoc />
        internal virtual void InternalPhysicsProcess(float delta)
        {
            if (this.isInit == true)
            {
                //physics tick replacement
                var tickInterval = (float)this.GetPhysicsProcessDeltaTime();
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
