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

using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;
namespace Framework.Game.Server
{
    /// <summary>
    /// Core class for server player
    /// </summary>
    public class ServerPlayer : PhysicsPlayer
    {
        /// <inheritdoc />
        public ServerPlayer() : base()
        {
        }

        /// <summary>
        /// Time since last disconnect
        /// </summary>
        /// <value></value>
        public float DisconnectTime { get; set; } = 0;

        /// <summary>
        /// Get the last tick of the last input
        /// </summary>
        /// <value></value>
        public uint LatestInputTick { get; set; } = 0;

        /// <summary>
        /// Return if the player is syncronized with server.
        /// </summary>
        /// <value></value>
        public bool IsSynchronized { get; set; } = false;


        /// <summary>
        /// Archived player states
        /// </summary>
        public PlayerState[] States = new PlayerState[1024];


        /// <summary>
        /// The active tick based input
        /// </summary>
        /// <value></value>
        // Current input struct for each player.
        // This is only needed because the ProcessAttack delegate flow is a bit too complicated.
        // TODO: Simplify this.
        public TickInput CurrentPlayerInput { get; set; }

        /// <inheritdoc />
        public string[] RequiredPuppetComponents { get; set; } = new string[0];


    }
}
