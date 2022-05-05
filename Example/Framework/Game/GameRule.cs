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
using Framework.Physics;
using Framework.Game.Server;

namespace Framework.Game
{
    /// <summary>
    /// Basic class for an game rule
    /// </summary>
    public abstract class GameRule : IGameRule
    {
        /// <inheritdoc />
        public string RuleName { get; set; }

        /// <inheritdoc />
        public NetworkServerWorld GameWorld { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameRule()
        {

        }
        /// <inheritdoc />
        public virtual void OnGameRuleActivated()
        {

        }

        /// <inheritdoc />
        public virtual void OnNewPlayerJoined(INetworkCharacter player)
        {

        }


        /// <inheritdoc />
        public virtual void OnPlayerRejoined(INetworkCharacter player)
        {

        }


        /// <inheritdoc />
        public virtual void OnPlayerLeaveTemporary(INetworkCharacter player)
        {

        }

        /// <inheritdoc />
        public virtual void OnPlayerLeave(INetworkCharacter player)
        {

        }


        /// <inheritdoc />
        public virtual void Tick(float delta)
        {

        }

        /// <summary>
        /// Trigger when an player got an hit
        /// </summary>
        public virtual void OnHit(RayCastHit player)
        {

        }

        /// <inheritdoc />
        public void AddComponentToServerPlayer<T>(INetworkCharacter player) where T : IPlayerComponent
        {
            if (player is NetworkCharacter)
            {
                var compIndex = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                if (compIndex > -1)
                {
                    var component = player.AvaiablePlayerComponents[compIndex];

                    Logger.LogDebug(this, "Found registry component: " + component.NodeType.ToString() + " => " + component.ResourcePath);

                    var playerInstance = player as NetworkCharacter;
                    var playerComponents = playerInstance.Components;

                    player.AddAssignedComponent(component);

                }
            }
        }

        /// <inheritdoc />
        public void AddComponentToLocalPlayer<T>(INetworkCharacter player) where T : IPlayerComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                var list = (player as NetworkCharacter).RequiredComponents.ToList();
                if (index > -1)
                {
                    list.Add(index);
                    (player as NetworkCharacter).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void AddComponentToPuppetPlayer<T>(INetworkCharacter player) where T : IPlayerComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                var list = (player as NetworkCharacter).RequiredPuppetComponents.ToList();
                if (index > -1)
                {
                    list.Add(index);
                    (player as NetworkCharacter).RequiredPuppetComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoveComponentFromLocalPlayer<T>(INetworkCharacter player) where T : IPlayerComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));

                var list = (player as NetworkCharacter).RequiredComponents.ToList();
                if (index > -1)
                {
                    list.Remove(index);
                    (player as NetworkCharacter).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoteComponentFromPuppetPlayer<T>(INetworkCharacter player) where T : IPlayerComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));

                var list = (player as NetworkCharacter).RequiredPuppetComponents.ToList();
                if (index > -1)
                {
                    list.Remove(index);
                    (player as NetworkCharacter).RequiredPuppetComponents = list.ToArray();
                }
            }
        }
    }
}