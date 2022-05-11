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
using Framework.Game.Client;
using Framework.Network.Commands;
using System.Linq;
using Framework.Game;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Framework.Input;

namespace Framework.Game
{
    /// <summary>
    /// The character or kinematic 3d body node for an network player
    /// </summary>
    public partial class NetworkInput : Node3D, IInputProcessor
    {
        /// <inheritdoc />
        [Export]
        public bool IsEnabled { get; set; } = false;

        /// <inheritdoc />
        public short NetworkId { get; set; } = -7;

        /// <summary>
        /// The last player input
        /// </summary>
        /// <value></value>
        public GeneralPlayerInput LastInput { get; private set; }

        public void SetPlayerInputs(GeneralPlayerInput inputs)
        {
            this.LastInput = inputs;
        }

        /// <summary>
        /// Get the current player input
        /// </summary>
        /// <value></value>
        public virtual GeneralPlayerInput GetInput()
        {
            var camera = this.BaseComponent.Components.Get<CharacterCamera>();
            var input = new GeneralPlayerInput();
            var activeKeys = new Dictionary<string, bool>();
            if (Godot.Input.GetMouseMode() == Godot.Input.MouseMode.Captured && this.IsEnabled)
            {
                activeKeys = this.GetKeys();
            }

            input.ViewDirection = camera != null ? camera.GetViewRotation() : Vector3.Zero;
            input.Apply(this.AvaiableInputs, activeKeys);

            return input;
        }

        /// <inheritdoc />
        public NetworkCharacter BaseComponent { get; set; }

        /// <inheritdoc />
        public void Tick(float delta)
        {

        }

        private bool lastJump = false;

        /// <inheritdoc />
        public List<string> AvaiableInputs => this.GetKeys().Keys.ToList();

        /// <inheritdoc />  
        public virtual Dictionary<String, bool> GetKeys()
        {
            var newJump = Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_jump", Godot.Key.Space);

            var canJump = false;
            if (newJump == true && lastJump == false)
            {
                canJump = true;
            }
            else if (newJump == true && lastJump == true)
            {
                canJump = false;
            }
            else if (newJump == false && lastJump == true)
            {
                canJump = false;
            }

            lastJump = newJump;

            return new Dictionary<string, bool>{
                { "Forward", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_forward", Godot.Key.W)},
                { "Back", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_backward", Godot.Key.S)},
                { "Right", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_right", Godot.Key.D)},
                { "Left", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_left", Godot.Key.A)},
                { "Jump", canJump},
                { "Crouch", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_crouch", Godot.Key.Ctrl)},
                { "Shifting", Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_shift", Godot.Key.Shift)},
                { "Fire",  Framework.Game.Client.ClientSettings.Variables.IsKeyValuePressed("key_attack", Godot.MouseButton.Left)},
            };
        }

    }
}
