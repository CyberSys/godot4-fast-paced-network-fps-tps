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

using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using System.Linq;
using Framework.Network;
using System;
using Godot;
using Framework.Physics;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Framework.Physics.Commands;

namespace Framework.Game.Client
{
    /// <summary>
    /// The base class of an puppet player
    /// </summary>
    public class PuppetPlayer : NetworkPlayer
    {
        /// <inheritdoc />
        public PuppetPlayer() : base()
        {
        }

        private Queue<PlayerState> stateQueue = new Queue<PlayerState>();
        private PlayerState? lastState = null;
        private float stateTimer = 0;

        /// <inheritdoc />
        internal override void InternalTick(float delta)
        {
            base.InternalTick(delta);

            if (!this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                return;
            }

            stateTimer += delta;

            float SimulationTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            float ServerSendRate = SimulationTickRate / 2;
            float ServerSendInterval = 1f / ServerSendRate;

            if (stateTimer > ServerSendInterval)
            {
                stateTimer -= ServerSendInterval;
                if (stateQueue.Count > 1)
                {
                    lastState = stateQueue.Dequeue();
                }
            }

            // We can only interpolate if we have a previous and next world state.
            if (!lastState.HasValue || stateQueue.Count < 1)
            {
                Logger.LogDebug(this, "RemotePlayer: not enough states to interp");
                return;
            }

            var nextState = stateQueue.Peek();
            float theta = stateTimer / ServerSendInterval;

            //only for movement component (to interpolate)
            if (this.Body != null)
            {
                var decomposedNextState = nextState.BodyComponent;
                var decomposesLastState = lastState.Value.BodyComponent;

                var a = decomposesLastState.Rotation;
                var b = decomposedNextState.Rotation;

                var newState = this.Body.GetNetworkState();

                newState.Position = decomposesLastState.Position.Lerp(decomposedNextState.Position, theta);
                newState.Rotation = a.Slerp(b, theta);

                this.Body.ApplyNetworkState(newState);
            }

            //for the rest -> just handle applying components
            foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
            {

                var index = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());

                if (index < 0)
                    continue;


                if (nextState.NetworkComponents.ContainsKey(index))
                {
                    var instanceMethod = component.GetType().GetMethod("ApplyNetworkState");
                    var parameterType = instanceMethod.GetParameters().First().ParameterType;

                    var methods = nextState.GetType().GetMethods();
                    var method = methods.Single(mi => mi.Name == "Decompose" && mi.GetParameters().Count() == 1);

                    var decomposed = method.MakeGenericMethod(parameterType)
                          .Invoke(nextState, new object[] { index });
                    instanceMethod.Invoke(component, new object[] { decomposed });
                }
            }
        }

        /// <inheritdoc />
        public override void ApplyNetworkState(PlayerState state)
        {
            if (this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                while (stateQueue.Count >= 2)
                {
                    stateQueue.Dequeue();
                }
                stateQueue.Enqueue(state);
            }
            else
            {
                base.ApplyNetworkState(state);
            }
        }
    }
}
