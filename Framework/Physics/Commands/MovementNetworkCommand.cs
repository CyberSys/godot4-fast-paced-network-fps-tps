using System.Linq;
using System.Collections.Generic;
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
using System.Reflection;
using LiteNetLib.Utils;
using System;

namespace Framework.Physics.Commands
{
    /// <summary>
    /// The default network command for moving bodies
    /// </summary>
    public struct MovementNetworkCommand : INetSerializable
    {
        /// <summary>
        /// The current position of this player
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The current rotation of this player
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// The current velocity of this player
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// If player is grounded or not
        /// </summary>
        public bool Grounded;

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.Position);
            writer.Put(this.Rotation);
            writer.Put(this.Velocity);
            writer.Put(this.Grounded);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.Position = reader.GetVector3();
            this.Rotation = reader.GetQuaternion();
            this.Velocity = reader.GetVector3();
            this.Grounded = reader.GetBool();
        }
    }
}