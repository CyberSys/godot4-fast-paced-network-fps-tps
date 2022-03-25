using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Shooter.Shared
{
    public interface ISimulationAdjuster
    {
        float AdjustedInterval { get; }
    }

    public class NoopAdjuster : ISimulationAdjuster
    {
        public float AdjustedInterval { get; } = 1.0f;
    }

    public abstract partial class GameWorld : Node3D, IWorld
    {
        public uint WorldTick { get; protected set; } = 0;
        protected virtual void PostUpdate() { }


        private InterpolationController interpController = new InterpolationController();

        protected Node playerHolder = new Node();
        protected AsyncLoader loader = new AsyncLoader();
        protected CoreGameLogic gameInstance = null;
        private float accumulator;
        protected float adjustedInterval = 1.0f;
        protected ISimulationAdjuster simulationAdjuster = new NoopAdjuster();

        protected GameLevel level;
        public GameLevel Level => level;

        public void InstanceLevel(PackedScene res)
        {
            var node = res.Instantiate<GameLevel>();
            node.Name = "level";
            this.AddChild(node);

            this.level = node;
        }

        public virtual void Init(uint initalWorldTick)
        {
            this.isInit = true;
        }

        public virtual void Tick(float interval)
        {
            ++this.WorldTick;
        }

        protected bool isInit = false;

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
            this.gameInstance = this.GetParent<CoreGameLogic>();

            this.AddChild(playerHolder);
        }

        public T AddPlayerSimulation<T>(int id) where T : PlayerSimulation
        {
            if (!playerHolder.HasNode(id.ToString()))
            {
                T simulation = Activator.CreateInstance<T>();
                simulation.Name = id.ToString();
                simulation.GameWorld = this;

                playerHolder.AddChild(simulation);

                return simulation;
            }
            else
            {
                return null;
            }
        }

        public void RemovePlayerSimulation(int id)
        {
            if (playerHolder.HasNode(id.ToString()))
            {
                playerHolder.GetNode(id.ToString()).QueueFree();
            }
        }

        public void Destroy()
        {
            this.QueueFree();
        }


        public PlayerSimulation GetPlayerSimulation(int id)
        {
            if (playerHolder.HasNode(id.ToString()))
            {
                return playerHolder.GetNode<PlayerSimulation>(id.ToString());
            }
            else
            {
                return null;
            }
        }

    }
}
