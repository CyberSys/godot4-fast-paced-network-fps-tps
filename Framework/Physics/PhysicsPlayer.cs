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

using Framework.Network;
using Framework.Input;
using Godot;
using Framework.Game;
using Framework.Network.Commands;

namespace Framework.Physics
{
    /// <summary>
    /// The base class for physics based players (kinematic, rigid..)
    /// </summary>
    public abstract partial class PhysicsPlayer : NetworkPlayer
    {

        /// <summary>
        /// The player camera component
        /// </summary>
        /// <value></value>
        public PhysicsPlayerCamera Camera { get; set; } = null;

        /// <summary>
        /// Contains the input proecessor
        /// </summary>
        /// <returns></returns>
        public IInputProcessor InputProcessor { get; set; } = new GeneralInputProcessor();

        /// <summary>
        /// Movement processor
        /// </summary>
        /// <returns></returns>
        public IMovementProcessor MovementProcessor { get; set; } = new DefaultMovementProcessor();


        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void DoTeleport(Godot.Vector3 origin)
        {
            if (this.Body != null)
            {
                var data = this.Body.GetNetworkState();
                data.Position = origin;
                this.Body.ApplyNetworkState(data);
            }
        }

        /// <summary>
        /// Current player head height
        /// </summary>
        public float PlayerHeadHeight { get; set; } = 1.5f;


        /// <summary>
        /// The last player input
        /// </summary>
        /// <value></value>
        public GeneralPlayerInput LastInput { get; private set; }

        internal void SetPlayerInputs(GeneralPlayerInput inputs)
        {
            this.LastInput = inputs;
        }

        /// <inheritdoc />
        internal override void InternalTick(float delta)
        {
            foreach (var component in this.Components.All)
            {
                if (component is IPlayerComponent)
                {
                    (component as IPlayerComponent).Tick(delta);
                }
            }

            if (this.Body != null)
            {
                this.MovementProcessor.SetServerVars(this.GameWorld.ServerVars);
                this.MovementProcessor.SetClientVars(Framework.Game.Client.ClientSettings.Variables);
                this.MovementProcessor.Simulate(this.Body, this.LastInput, delta);
            }

            base.InternalTick(delta);
        }

        /// <summary>
        /// Detect an hit by given camera view
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public virtual RayCastHit DetechtHit(float range)
        {
            var command = this.Body != null ? this.Body.GetNetworkState() : default(Physics.Commands.MovementNetworkCommand);
            var currentTransform = new Godot.Transform3D(command.Rotation, command.Position);
            var attackPosition = currentTransform.origin + Vector3.Up * this.PlayerHeadHeight + currentTransform.basis.x * 0.2f;
            var attackTransform = new Godot.Transform3D(command.Rotation, attackPosition);

            var raycast = new PhysicsRayQueryParameters3D();
            raycast.From = attackTransform.origin;
            raycast.To = attackTransform.origin + -attackTransform.basis.z * range;

            var result = GetWorld3d().DirectSpaceState.IntersectRay(raycast);

            if (result != null && result.Contains("position"))
            {
                var rayResult = new RayCastHit
                {
                    PlayerSource = this,
                    To = (Vector3)result["position"],
                    Collider = (Node)result["collider"],
                    From = attackTransform.origin
                };

                if (rayResult.Collider is HitBox)
                {
                    var enemy = (rayResult.Collider as HitBox).GetPlayer();
                    if (enemy != null && enemy is IPlayer)
                    {
                        rayResult.PlayerDestination = enemy;
                    }
                }

                return rayResult;
            }
            else return null;
        }

        /// <summary>
        /// Trigger when an player got an hit
        /// </summary>
        public virtual void OnHit(RayCastHit hit)
        {

        }
    }
}
