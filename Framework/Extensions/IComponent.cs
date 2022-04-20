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

namespace Framework
{
    /// <summary>
    /// The base component interface required for root components 
    /// </summary>
    public interface IBaseComponent
    {
        /// <summary>
        /// Add an child to the base component
        /// </summary>
        /// <param name="node"></param>
        /// <param name="legibleUniqueName"></param>
        /// <param name="internal"></param>
        public void AddChild(Node node, bool legibleUniqueName = false, Node.InternalMode @internal = Node.InternalMode.Disabled);

        /// <summary>
        /// Delete an child form the base component
        /// </summary>
        /// <param name="path"></param>
        public void RemoveChild(Node path);

        /// <summary>
        /// Check if the component is already inside an tree
        /// </summary>
        /// <returns></returns>
        public bool IsInsideTree();

        /// <summary>
        /// Handle the tree events of godot node tree
        /// </summary>
        public event Godot.Node.TreeEnteredHandler TreeEntered;

        /// <summary>
        /// Contains the component registry class
        /// </summary>
        /// <value></value>
        public ComponentRegistry Components { get; }
    }

    public static class IPlayerComponentExtensions
    {
        public static bool IsServer(this IPlayerComponent client)
        {
            return (client.BaseComponent is Framework.Game.Server.ServerPlayer);
        }

        public static bool IsLocal(this IPlayerComponent client)
        {
            return (client.BaseComponent is Framework.Game.Client.LocalPlayer);
        }

        public static bool IsPuppet(this IPlayerComponent client)
        {
            return (client.BaseComponent is Framework.Game.Client.PuppetPlayer);
        }
    }

    /// <summary>
    /// Interface required for player components
    /// </summary>
    public interface IPlayerComponent : IChildComponent
    {
        /// <summary>
        /// Called on each physics network tick for component
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);
    }


    /// <summary>
    /// The child component interface for childs of an base component
    /// </summary>
    public interface IChildComponent
    {
        /// <summary>
        /// The base component of the child component
        /// </summary>
        /// <value></value>
        public IBaseComponent BaseComponent { get; set; }
    }
}
