using System.Linq;
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
using Framework.Network.Commands;
using Framework.Network;
using LiteNetLib.Utils;
using Framework.Input;
using System;
using Framework.Game.Client;

namespace Framework.Physics
{
    /// <summary>
    /// The base class for physics based players (kinematic, rigid..)
    /// </summary>
    public abstract partial class PhysicsPlayer : NetworkPlayer
    {
        /// <inheritdoc />
        public PhysicsPlayer() : base()
        {
        }

        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void DoTeleport(Godot.Vector3 origin)
        {
            foreach (var component in this.Components.All)
            {
                if (component is IChildMovementNetworkSyncComponent)
                {
                    var data = (component as IChildMovementNetworkSyncComponent).GetNetworkState();
                    data.Position = origin;
                    (component as IChildMovementNetworkSyncComponent).ApplyNetworkState(data);
                }
            }
        }

        internal GeneralPlayerInput inputs;
        public void SetPlayerInputs(GeneralPlayerInput inputs)
        {
            this.inputs = inputs;
        }

        /// <inheritdoc />
        internal override void InternalTick(float delta)
        {
            base.InternalTick(delta);

            foreach (var component in this.Components.All)
            {
                if (component is IPlayerComponent)
                {
                    (component as IPlayerComponent).Tick(delta);
                }

                if (component is IChildMovementNetworkSyncComponent)
                {
                    var bodyComp = (component as IChildMovementNetworkSyncComponent);

                    bodyComp.MovementProcessor.SetServerVars(this.GameWorld.ServerVars);
                    bodyComp.MovementProcessor.SetClientVars(Framework.Game.Client.ClientSettings.Variables);
                    bodyComp.MovementProcessor.Simulate(component as IChildMovementNetworkSyncComponent, this.inputs, delta);
                }
            }
        }
    }
}
