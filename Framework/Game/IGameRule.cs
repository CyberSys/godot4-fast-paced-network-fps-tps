using System.Linq;
using Framework.Game;
using System;
using System.Collections.Generic;
using LiteNetLib;
using Framework.Utils;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Network.Services;
using Framework.Input;
using Framework.Physics;
using Framework.Game.Server;
using System.Reflection;

namespace Framework.Game
{
    /// <summary>
    /// Required interface for game rules
    /// </summary>
    public interface IGameRule
    {
        /// <summary>
        /// The server world
        /// </summary>
        /// <value></value>
        public ServerWorld GameWorld { get; }

        /// <summary>
        /// Called on new player joined the game
        /// </summary>
        /// <param name="player"></param>
        public void OnNewPlayerJoined(IPlayer player);

        /// <summary>
        /// Called when player are rejoined the game (after previous disconnect)
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerRejoined(IPlayer player);

        /// <summary>
        /// Called when players are disconnected (as eg. timeouts)
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeaveTemporary(IPlayer player);

        /// <summary>
        /// Called when players finanly leave the game
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeave(IPlayer player);

        /// <summary>
        /// Execute on each server tick
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);

        /// <summary>
        /// Add an component to an client player instance
        /// </summary>
        /// <param name="player"></param>
        /// <param name="component"></param>
        public void AddRemoteComponentToPlayer(IPlayer player, string component);

        /// <summary>
        /// Add an component to an server player instance
        /// </summary>
        /// <param name="player"></param>
        /// <param name="component"></param>
        public void AddComponentToPlayer(IPlayer player, string component);
    }
}