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
        /// Return the childrens of an component
        /// </summary>
        /// <param name="includeInternal"></param>
        /// <returns></returns>
        public Godot.Collections.Array GetChildren(bool includeInternal = false);

        /// <summary>
        /// Handle the tree events of godot node tree
        /// </summary>
        public event Godot.Node.TreeEnteredHandler TreeEntered;
    }

    /// <inheritdoc />
    public static class IPlayerComponentExtensions
    {
        /// <inheritdoc />
        public static bool IsServer(this IPlayerComponent client)
        {
            return client.BaseComponent.IsServer();
        }

        /// <inheritdoc />
        public static bool IsLocal(this IPlayerComponent client)
        {
            return client.BaseComponent.IsLocal();
        }

        /// <inheritdoc />
        public static bool IsPuppet(this IPlayerComponent client)
        {
            return client.BaseComponent.IsPuppet();
        }
    }

    /// <summary>
    /// Interface required for player components
    /// </summary>
    public interface IPlayerComponent : IChildComponent<Framework.Game.NetworkCharacter>
    {
        /// <summary>
        /// If the component is enabled or not
        /// </summary>
        /// <value></value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The component id for network transfer
        /// </summary>
        /// <value></value>
        public short NetworkId { get; set; }

        /// <summary>
        /// Called on each physics network tick for component
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);
    }


    /// <summary>
    /// The child component interface for childs of an base component
    /// </summary>
    public interface IChildComponent<T> where T : IBaseComponent
    {
        /// <summary>
        /// Check if the child is inside the engine tree
        /// </summary>
        /// <returns></returns>
        public bool IsInsideTree();

        /// <summary>
        /// The bas component
        /// </summary>
        /// <value></value>
        public T BaseComponent { get; set; }

    }
}
