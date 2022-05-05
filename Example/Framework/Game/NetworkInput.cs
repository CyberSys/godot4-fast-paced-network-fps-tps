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
    public partial class NetworkInput : Node, IPlayerComponent
    {
        /// <summary>
        /// Contains the input proecessor
        /// </summary>
        /// <returns></returns>
        public IInputProcessor InputProcessor { get; set; } = new GeneralInputProcessor();

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
        public NetworkCharacter BaseComponent { get; set; }

        /// <inheritdoc />
        public void Tick(float delta)
        {

        }
    }
}
