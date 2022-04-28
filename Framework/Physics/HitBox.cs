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
    /// Hit box collider
    /// </summary>
    public class HitBox : StaticBody3D
    {
        /// <summary>
        /// The name of the collider group
        /// </summary>

        [Export]
        public string GroupName { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.CollisionLayer = 0;
            this.CollisionMask = 0;
            this.SetCollisionLayerValue(32, true);
            this.SetCollisionMaskValue(32, true);
        }

        /// <summary>
        /// Get the player instance of an hitbox
        /// </summary>
        /// <returns></returns>
        public IPlayer GetPlayer()
        {
            return this.FindPlayer(this);
        }

        internal IPlayer FindPlayer(Node n)
        {
            if (n is IPlayer)
            {
                return n as IPlayer;
            }
            else if (this.GetParent() != null && this.GetParent() is Viewport)
            {
                return null;
            }
            else if (this.GetParent() != null && this.GetParent() is Node)
            {
                return this.FindPlayer(this.GetParent() as Node);
            }
            else
            {
                return null;
            }
        }
    }
}