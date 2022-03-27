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

    public struct RegisteredComonent
    {
        public Type NodeType { get; set; }
        public string ResourcePath { get; set; }

        public RegisteredComonent(Type NodeType, string ResourcePath = null)
        {
            this.NodeType = NodeType;
            this.ResourcePath = ResourcePath;
        }
    }
}