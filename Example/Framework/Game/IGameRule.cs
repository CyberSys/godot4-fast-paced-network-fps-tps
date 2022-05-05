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
        /// The name of the game rule
        /// </summary>
        /// <value></value>
        public string RuleName { get; }

        /// <summary>
        /// The server world
        /// </summary>
        /// <value></value>
        public NetworkServerWorld GameWorld { get; }

        /// <summary>
        /// Called on new player joined the game
        /// </summary>
        /// <param name="player"></param>
        public void OnNewPlayerJoined(INetworkCharacter player);

        /// <summary>
        /// Triggered when the game rule will be activated
        /// </summary>
        public void OnGameRuleActivated();

        /// <summary>
        /// Triggered when an player got an hit
        /// </summary>
        /// <param name="player"></param>
        public void OnHit(RayCastHit player);

        /// <summary>
        /// Called when player are rejoined the game (after previous disconnect)
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerRejoined(INetworkCharacter player);

        /// <summary>
        /// Called when players are disconnected (as eg. timeouts)
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeaveTemporary(INetworkCharacter player);

        /// <summary>
        /// Called when players finanly leave the game
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerLeave(INetworkCharacter player);

        /// <summary>
        /// Execute on each server tick
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);

        /// <summary>
        /// Add an component to an local player instance on client side
        /// </summary>
        /// <param name="player"></param>
        /// <typeparam name="T">Type of component</typeparam>
        public void AddComponentToLocalPlayer<T>(INetworkCharacter player) where T : IPlayerComponent;

        /// <summary>
        /// Add an component to an server player instance
        /// </summary>
        /// <param name="player"></param>
        /// <typeparam name="T">Type of component</typeparam>
        public void AddComponentToPuppetPlayer<T>(INetworkCharacter player) where T : IPlayerComponent;

        /// <summary>
        /// Add an component to an server side player
        /// </summary>
        /// <param name="player"></param>
        /// <typeparam name="T">Type of component</typeparam>
        public void AddComponentToServerPlayer<T>(INetworkCharacter player) where T : IPlayerComponent;


        /// <summary>
        /// Remove an component from an local player (client sided)
        /// </summary>
        /// <param name="player"></param>
        /// <typeparam name="T">Type of component</typeparam>
        public void RemoveComponentFromLocalPlayer<T>(INetworkCharacter player) where T : IPlayerComponent;

        /// <summary>
        /// Remove an component from an local player (server sided)
        /// </summary>
        /// <param name="player"></param>
        /// <typeparam name="T">Type of component</typeparam>
        public void RemoteComponentFromPuppetPlayer<T>(INetworkCharacter player) where T : IPlayerComponent;
    }
}