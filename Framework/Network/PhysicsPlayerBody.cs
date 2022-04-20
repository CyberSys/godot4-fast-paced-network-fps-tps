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

using Framework.Physics;
using Godot;
using System;
using Framework.Physics.Commands;

namespace Framework.Network
{
    /// <summary>
    /// The character or kinematic 3d body node for an network player
    /// </summary>
    public partial class NetworkPlayerBody : CharacterBody3D, IPlayerComponent
    {
        /// <inheritdoc />
        public IBaseComponent BaseComponent { get; set; }

        /// <summary>
        /// Node path to collider
        /// </summary>
        [Export]
        public NodePath ColliderPath = null;

        /// <summary>
        /// The height on crouching
        /// </summary>
        [Export]
        public float CrouchHeight = 1.3f;

        internal float originalHeight = 0f;

        internal CollisionShape3D shape;

        internal float originalYPosition = 0f;
        internal float shapeHeight = 0.0f;
        internal float previousCrouchLevel = 0f;
        internal float currentCouchLevel = 0f;

        /// <summary>
        /// Get the current shape height
        /// </summary>
        /// <returns></returns>
        public float GetShapeHeight()
        {
            if (this.shape != null)
                return this.shape.Transform.origin.y;
            else
                return 0;
        }

        /// <inheritdoc />
        public virtual void Tick(float delta)
        {

        }

        /// <summary>
        /// Apply the network state for the body component
        /// </summary>
        /// <param name="state"></param>
        public void ApplyNetworkState(MovementNetworkCommand state)
        {
            var transform = this.Transform;
            transform.origin = state.Position;
            transform.basis = new Basis(state.Rotation);
            this.Transform = transform;
            this.Velocity = state.Velocity;

            if (this.BaseComponent is PhysicsPlayer)
            {
                (this.BaseComponent as PhysicsPlayer).MovementProcessor.Velocity = state.Velocity;
            }
        }

        /// <summary>
        /// Get the current physics state for the body component
        /// </summary>
        /// <returns></returns>
        public MovementNetworkCommand GetNetworkState()
        {
            return new MovementNetworkCommand
            {
                Position = this.Transform.origin,
                Rotation = this.Transform.basis.GetRotationQuaternion(),
                Velocity = this.Velocity,
                Grounded = this.IsOnGround(),
            };
        }

        /// <inheritdoc />
        public override void _EnterTree()
        {
            base._EnterTree();

            this.shape = this.GetNodeOrNull<CollisionShape3D>(ColliderPath);

            float shapeHeight = 0;

            if (shape != null)
            {
                this.originalHeight = this.shape.Scale.y;
                this.originalYPosition = this.shape.Transform.origin.y;
                if (this.shape.Shape is CapsuleShape3D)
                {
                    shapeHeight = (this.shape.Shape as CapsuleShape3D).Height;
                }

                else if (this.shape.Shape is BoxShape3D)
                {
                    shapeHeight = (this.shape.Shape as BoxShape3D).Size.y;
                }
                else
                {
                    throw new Exception("Shape type not implemented yet");
                }
            }

            this.shapeHeight = shapeHeight;
            this.currentCouchLevel = shapeHeight;
            this.previousCrouchLevel = shapeHeight;
        }

        /// <summary>
        /// Is player on ground
        /// </summary>
        /// <returns>true or false</returns>
        public virtual bool IsOnGround()
        {
            return this.IsOnFloor();
        }

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            base._Process(delta);

            if (this.IsLocal())
            {
                var local = this.BaseComponent as Framework.Game.Client.LocalPlayer;
                var shadowMode = GeometryInstance3D.ShadowCastingSetting.On;

                if (local.Camera != null && local.Camera.IsInsideTree() && local.Camera.Mode == CameraMode.FPS)
                {
                    shadowMode = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
                }

                this.SetShadowModes(this, shadowMode);
            }
        }

        internal void SetShadowModes(Node t, GeometryInstance3D.ShadowCastingSetting mode)
        {
            if (t is MeshInstance3D)
            {
                (t as MeshInstance3D).CastShadow = mode;
            }

            foreach (var element in t.GetChildren())
            {
                if (element is Node)
                {
                    SetShadowModes(element as Node, mode);
                }
            }
        }

        /// <summary>
        /// Moving the character
        /// </summary>
        /// <param name="delta">float</param>
        /// <param name="velocity">Vector3</param>
        public virtual void Move(float delta, Vector3 velocity)
        {
            this.MoveAndSlide();
        }

        /// <inheritdoc />
        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (this.previousCrouchLevel != this.currentCouchLevel)
            {
                if (this.shape != null && this.shape.Shape != null)
                {

                }
                var yPos = (this.shapeHeight - this.currentCouchLevel) / 2;
                yPos = yPos * -1;

                var transform = this.shape.Transform;
                transform.origin.y = this.originalYPosition + yPos;
                transform.origin.y = Mathf.Clamp(transform.origin.y, (this.shapeHeight / 2) * -1f, this.originalYPosition);
                this.shape.Transform = transform;

                var shape = this.shape.Shape as CapsuleShape3D;
                shape.Height = currentCouchLevel;
                this.shape.Shape = shape;

                this.previousCrouchLevel = this.currentCouchLevel;
            }
        }


        /// <summary>
        /// Set the shape height by a given process value 
        /// </summary>
        /// <param name="crouchInProcess">can be between 0 (crouching) and 1 (non crouching)</param>
        public virtual void SetCrouchingLevel(float crouchInProcess)
        {
            this.currentCouchLevel += MathF.Round(crouchInProcess, 2);
            this.currentCouchLevel = Mathf.Clamp(this.currentCouchLevel, CrouchHeight, this.shapeHeight);
        }
    }
}
