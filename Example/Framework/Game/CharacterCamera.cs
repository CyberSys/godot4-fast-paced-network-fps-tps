using System;
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
using Framework.Game.Client;
using Framework.Utils;
using Framework.Physics;
namespace Framework.Game
{
    /// <summary>
    /// The camera mode for the player camera
    /// </summary>
    public enum CameraMode
    {
        /// <summary>
        /// FPS Mode
        /// </summary>
        FPS,
        /// <summary>
        /// TPS Mode
        /// </summary>
        TPS,
        /// <summary>
        /// Follow player mode
        /// </summary>
        Follow,
        /// <summary>
        /// Dungeon camera mode
        /// </summary>
        Dungeon
    }

    /// <summary>
    /// The player camera for an physics player
    /// </summary>
    public partial class CharacterCamera : Camera3D, IPlayerComponent
    {

        [Export]
        /// <inheritdoc />
        public bool IsEnabled { get; set; } = false;

        /// <inheritdoc />
        public short NetworkId { get; set; } = -5;

        internal float tempRotX = 0.0f;
        internal float tempRotY = 0.0f;
        internal float tempRotZ = 0.0f;
        internal float tempYaw = 0.0f;
        internal float tempPitch = 0.0f;

        /// <summary>
        /// The base component of the child component
        /// </summary>
        /// <value></value>
        public Framework.Game.NetworkCharacter BaseComponent { get; set; }

        /// <summary>
        /// The current camera mode in use
        /// </summary>
        /// /// <value></value>
        [Export(PropertyHint.Enum)]
        public CameraMode Mode { get; set; } = CameraMode.FPS;

        /// <summary>
        /// The Camera Distance from the character in TPS Mode
        /// </summary>
        [Export]
        public Vector3 TPSCameraOffset = new Vector3(0, 0.5f, 0);

        /// <summary>
        /// The Camera Radis
        /// </summary>
        [Export]
        public float TPSCameraRadius = 1.7f;


        public Vector3 FPSCameraOffset = Vector3.Zero;


        /// <summary>
        /// Called on each physics network tick for component
        /// </summary>
        /// <param name="delta"></param>
        public virtual void Tick(float delta)
        {
        }

        /// <inheritdoc />
        public override void _EnterTree()
        {
            base._EnterTree();

            var rotation = this.GlobalTransform.basis.GetEuler();

            this.tempRotX = rotation.x;
            this.tempRotY = rotation.y;
            this.tempRotZ = rotation.z;

            this.Current = this.IsEnabled;
            this.TopLevel = true;
            this.FPSCameraOffset = this.Position;
        }

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            base._Process(delta);

            if (this.BaseComponent == null)
                return;

            this.Current = (!this.IsPuppet() && IsEnabled);

            if (this.IsServer())
            {
                var transform = this.BaseComponent.GlobalTransform;
                var targetPos = this.BaseComponent.GlobalTransform.origin + FPSCameraOffset + Vector3.Up * this.BaseComponent.GetShapeHeight();
                transform.origin = targetPos;
                transform.basis = new Basis(new Vector3(this.BaseComponent.CurrentPlayerInput.Inputs.ViewDirection.x, transform.basis.GetEuler().y, transform.basis.GetEuler().z));
                this.GlobalTransform = transform;

            }
            else if (this.Mode == CameraMode.TPS)
            {
                var cam_pos = this.BaseComponent.GlobalTransform.origin + TPSCameraOffset;
                if (!this.IsServer())
                {
                    cam_pos.x += TPSCameraRadius * Mathf.Sin(Mathf.Deg2Rad(tempYaw)) * Mathf.Cos(Mathf.Deg2Rad(tempPitch));
                    cam_pos.y += TPSCameraRadius * Mathf.Sin(Mathf.Deg2Rad(tempPitch));
                    cam_pos.z += TPSCameraRadius * Mathf.Cos(Mathf.Deg2Rad(tempYaw)) * Mathf.Cos(Mathf.Deg2Rad(tempPitch));

                    this.LookAtFromPosition(cam_pos, this.BaseComponent.GlobalTransform.origin + TPSCameraOffset, new Vector3(0, 1, 0));
                }
            }
            else if (this.Mode == CameraMode.FPS)
            {
                var transform = this.BaseComponent.GlobalTransform;

                var target = this.BaseComponent.GlobalTransform.origin + FPSCameraOffset + Vector3.Up * this.BaseComponent.GetShapeHeight();
                transform.origin = target;
                transform.basis = new Basis(new Vector3(tempRotX, tempRotY, 0));
                this.GlobalTransform = transform;
            }

            this.Fov = ClientSettings.Variables.Get<int>("cl_fov");
        }

        /// <inheritdoc />
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            this.HandleInput(@event);

            @event.Dispose();
        }

        /// <summary>
        /// Get the view rotation of an local player
        /// </summary>
        /// <returns></returns>
        public virtual Godot.Vector3 GetViewRotation()
        {
            return this.Transform.basis.GetEuler();
        }

        /// <inheritdoc />
        internal void HandleInput(InputEvent @event)
        {
            if (this.BaseComponent == null)
                return;

            if (this.IsLocal())
            {
                var sensX = ClientSettings.Variables.Get<float>("cl_sensitivity_y", 2.0f);
                var sensY = ClientSettings.Variables.Get<float>("cl_sensitivity_x", 2.0f);

                var input = BaseComponent.Components.Get<NetworkInput>();
                if (@event is InputEventMouseMotion && input != null && input.IsEnabled)
                {
                    // Handle cursor lock state
                    if (Godot.Input.GetMouseMode() == Godot.Input.MouseMode.Captured)
                    {
                        var ev = @event as InputEventMouseMotion;
                        tempRotX -= ev.Relative.y * (sensY / 100);
                        tempRotX = Mathf.Clamp(tempRotX, Mathf.Deg2Rad(-90), Mathf.Deg2Rad(90));
                        tempRotY -= ev.Relative.x * (sensX / 100);

                        tempYaw = (tempYaw - (ev.Relative.x * (sensX))) % 360;
                        tempPitch = Mathf.Max(Mathf.Min(tempPitch + (ev.Relative.y * (sensY)), 85), -85);
                    }
                }

                if (@event.IsActionReleased("camera"))
                {
                    if (this.Mode == CameraMode.FPS)
                    {
                        this.Mode = CameraMode.TPS;
                    }
                    else if (this.Mode == CameraMode.TPS)
                    {
                        this.Mode = CameraMode.FPS;
                    }
                }
            }
        }
    }

}
