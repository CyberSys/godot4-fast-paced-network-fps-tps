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
using System.Collections.Generic;

namespace Framework.Game
{
    /// <summary>
    /// Required interface for players
    /// </summary>
    public interface IPlayer : IBaseComponent
    {
        /// <summary>
        /// Id of player
        /// </summary>
        /// <value></value>
        public int Id { get; set; }

        /// <summary>
        /// Name of player
        /// </summary>
        /// <value></value>
        public string PlayerName { get; set; }

        /// <summary>
        /// Current latency (ping)
        /// </summary>
        /// <value></value>
        public int Latency { get; set; }

        /// <summary>
        /// Current connection state
        /// </summary>
        /// <value></value>
        public PlayerConnectionState State { get; set; }

        /// <summary>
        /// Assigned team of player
        /// </summary>
        /// <value></value>
        public PlayerTeam Team { get; set; }

        /// <summary>
        /// Execute each server tick
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);

        /// <summary>
        /// Contains possible components
        /// </summary>
        /// <value></value>
        public Dictionary<string, AssignedComponent> AvaiablePlayerComponents { get; }

        /// <summary>
        /// List of required components for this player instance
        /// </summary>
        /// <value></value>
        public string[] RequiredComponents { get; set; }

        /// <summary>
        /// Add an avaiable component
        /// </summary>
        /// <param name="ResourcePath"></param>
        /// <typeparam name="T"></typeparam>
        public void AddAvaiableComponent<T>(string ResourcePath = null) where T : Godot.Node, IChildComponent, new();
    }
}
