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

    public abstract partial class World : Node3D, IWorld
    {
        public virtual void OnPlayerInitilaized(IPlayer p)
        {

        }

        public uint WorldTick { get; protected set; } = 0;

        protected readonly Dictionary<int, IPlayer> _players = new Dictionary<int, IPlayer>();
        public Dictionary<int, IPlayer> Players => _players;


        protected virtual void PostUpdate() { }

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

        protected void updatePlayerValues(PlayerUpdate playerUpdate, IPlayer player)
        {
            player.Team = playerUpdate.Team;
            player.Latency = playerUpdate.Latency;
            player.PlayerName = playerUpdate.PlayerName;
            player.State = playerUpdate.State;
        }

        public void InstanceLevel(PackedScene res)
        {
            var node = res.Instantiate<Level>();
            node.Name = "level";
            this.AddChild(node);

            this.level = node;
        }

        public virtual void Init(Dictionary<string, string> serverVars, uint initalWorldTick)
        {
            this.isInit = true;
            this._serverVars = serverVars;
            foreach (var sv in serverVars)
            {
                Logger.LogDebug(this, "[Config] " + sv.Key + " => " + sv.Value);
            }
        }

        public virtual void Tick(float interval)
        {
            ++this.WorldTick;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            //async loader
            this.loader.Tick();
        }

        public override void _PhysicsProcess(float delta)
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

                    Tick(tickInterval);
                }

                interpController.ExplicitUpdate(delta);
                PostUpdate();
            }
        }

        public override void _EnterTree()
        {
            playerHolder.Name = "playerHolder";

            base._EnterTree();
            this.gameInstance = this.GetParent<GameLogic>();

            this.AddChild(playerHolder);
        }

        public void Destroy()
        {
            this.QueueFree();
        }
    }
}
