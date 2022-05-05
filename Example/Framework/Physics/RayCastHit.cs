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
using Framework.Game;

namespace Framework.Physics
{
    /// <summary>
    /// The hit result of an ray cast
    /// </summary>
    public class RayCastHit
    {
        /// <summary>
        /// The 3d vector from where the raycast starts
        /// </summary>
        /// <value></value>
        public Vector3 From { get; set; }

        /// <summary>
        /// The 3d vector where the raycast ends
        /// </summary>
        /// <value></value>
        public Vector3 To { get; set; }


        /// <summary>
        /// The collider which hitted
        /// </summary>
        /// <value></value>
        public Node Collider { get; set; }


        /// <summary>
        /// The collider which hitted
        /// </summary>
        /// <value></value>
        public INetworkCharacter PlayerSource { get; set; }

        /// <summary>
        /// The collider which hitted
        /// </summary>
        /// <value></value>
        public INetworkCharacter PlayerDestination { get; set; }
    }
}