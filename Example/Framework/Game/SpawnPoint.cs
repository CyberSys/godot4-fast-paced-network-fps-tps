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

namespace Framework.Game
{
    /// <summary>
    /// The gernal spawn point class
    /// </summary>
    public partial class SpawnPoint : Area3D
    {
        /// <summary>
        /// Set or get the usage of the spawnpoint
        /// </summary>
        public bool inUsage { get; set; } = false;

        private int enteredBodies = 0;

        /// <summary>
        /// Returns if the spawn point are free and not in use
        /// </summary>
        /// <returns></returns>
        public bool isFree()
        {
            return (enteredBodies == 0);
        }

        /// <inheritdoc />
        public override void _EnterTree()
        {
            base._EnterTree();

            this.BodyEntered += (body) =>
            {
                if (body is RigidDynamicBody3D || body is CharacterBody3D)
                    enteredBodies++;
            };

            this.BodyExited += (body) =>
            {
                if (body is RigidDynamicBody3D || body is CharacterBody3D)
                    enteredBodies--;
            };
        }
    }
}
