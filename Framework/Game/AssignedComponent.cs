using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Network;
using System;
using Godot;
using Framework.Physics;
using System.Collections.Generic;

namespace Framework.Game
{

    /// <summary>
    /// An assignable component for remote component injection
    /// </summary>
    public struct AssignedComponent
    {
        /// <summary>
        /// The type of the child node which be injected to an remote base component (eg. player)
        /// </summary>
        /// <value></value>
        public Type NodeType { get; set; }

        /// <summary>
        /// The resourcepath (if exist) to the resource which will be injected
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Constrcutor for an assignable component
        /// </summary>
        /// <param name="NodeType">The type of the child node which be injected to an remote base component (eg. player)</param>
        /// <param name="ResourcePath">The resourcepath (if exist) to the resource which will be injected</param>
        public AssignedComponent(Type NodeType, string ResourcePath = null)
        {
            this.NodeType = NodeType;
            this.ResourcePath = ResourcePath;
        }
    }
}