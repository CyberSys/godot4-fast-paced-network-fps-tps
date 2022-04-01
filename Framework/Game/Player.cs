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
using Framework.Game;
using System.Collections.Generic;

namespace Framework.Game
{
    /// <summary>
    /// The general player class
    /// </summary>
    public abstract class Player : Godot.Node, IPlayer, IBaseComponent
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string PlayerName { get; set; }

        /// <inheritdoc />
        public int Latency { get; set; }

        /// <inheritdoc />
        public PlayerConnectionState State { get; set; }

        /// <inheritdoc />
        public PlayerTeam Team { get; set; }

        /// <inheritdoc />
        public IWorld GameWorld { get; set; }

        /// <inheritdoc />
        private readonly ComponentRegistry _components;

        /// <inheritdoc />
        public ComponentRegistry Components => _components;

        /// <summary>
        /// Previous state since last tick
        /// </summary>
        /// <value></value>
        public PlayerConnectionState PreviousState { get; set; }

        /// <summary>
        /// Base player class
        /// </summary>
        /// <returns></returns>
        public Player() : base()
        {
            this._components = new ComponentRegistry(this);
        }

        /// <inheritdoc />
        public virtual void Tick(float delta)
        {
        }

        internal virtual void InternalTick(float delta)
        {
            this.Tick(delta);
        }

        /// <inheritdoc />
        private readonly List<AssignedComponent> avaiableComponents = new List<AssignedComponent>();

        /// <inheritdoc />
        public List<AssignedComponent> AvaiablePlayerComponents => avaiableComponents;


        /// <inheritdoc />
        private readonly List<string> avaiableInputs = new List<string>();

        /// <inheritdoc />
        public List<string> AvaiableInputs => avaiableInputs;

        /// <inheritdoc />
        public void AddAvaiableComponent<T>(string ResourcePath = null) where T : Godot.Node, IChildComponent, new()
        {
            var element = new AssignedComponent(
                                typeof(T), ResourcePath
                );


            if (!this.avaiableComponents.Contains(element))
            {
                this.avaiableComponents.Add(element);
            }
        }

    }
}