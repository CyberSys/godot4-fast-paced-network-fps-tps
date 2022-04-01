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
using System.Reflection;
using Framework.Input;

namespace Framework.Game
{
    /// <summary>
    /// Basic class for an game rule
    /// </summary>
    public class GameRule : IGameRule
    {
        /// <inheritdoc />
        public ServerWorld GameWorld { get; private set; }

        /// <summary>
        /// Game rule constructor
        /// </summary>
        /// <param name="gameWorld">Game world</param>
        public GameRule(ServerWorld gameWorld)
        {
            GameWorld = gameWorld;
        }

        /// <inheritdoc />
        public virtual void OnNewPlayerJoined(IPlayer player)
        {

        }


        /// <inheritdoc />
        public virtual void OnPlayerRejoined(IPlayer player)
        {

        }


        /// <inheritdoc />
        public virtual void OnPlayerLeaveTemporary(IPlayer player)
        {

        }

        /// <inheritdoc />
        public virtual void OnPlayerLeave(IPlayer player)
        {

        }


        /// <inheritdoc />
        public virtual void Tick(float delta)
        {

        }

        /// <inheritdoc />
        public void AddComponentToServerPlayer(IPlayer player, string component)
        {
            if (player is Player)
            {
                var components = (player as Player).AvaiablePlayerComponents;
                if (components.ContainsKey(component) && player is Player)
                {
                    var registeredComonent = components[component];
                    Logger.LogDebug(this, "Found registry component: " + registeredComonent.NodeType.ToString() + " => " + registeredComonent.ResourcePath);

                    var playerInstance = player as Player;
                    var playerComponents = playerInstance.Components;

                    if (registeredComonent.ResourcePath != null)
                    {
                        this.CreateComponentWithResource(player, registeredComonent);
                    }
                    else
                    {
                        this.CreateComponent(player, registeredComonent);
                    }
                }
            }
        }

        private void CreateComponent(IPlayer player, AssignedComponent registeredComonent)
        {
            var method = typeof(ComponentRegistry).GetMethods().Single(
                       m =>
                       m.Name == "AddComponent" &&
                       m.GetGenericArguments().Length == 1 &&
                       m.GetParameters().Length == 0);

            MethodInfo generic = method.MakeGenericMethod(registeredComonent.NodeType);
            var createdObject = generic.Invoke(player.Components, new object[]
            {

            });
        }

        private void CreateComponentWithResource(IPlayer player, AssignedComponent registeredComonent)
        {
            var method = typeof(ComponentRegistry).GetMethods().Single(
                       m =>
                       m.Name == "AddComponent" &&
                       m.GetGenericArguments().Length == 1 &&
                       m.GetParameters().Length == 1 &&
                       m.GetParameters()[0].ParameterType == typeof(string));

            MethodInfo generic = method.MakeGenericMethod(registeredComonent.NodeType);
            var createdObject = generic.Invoke(player.Components, new object[]
            {
                        registeredComonent.ResourcePath
            });
        }

        /// <inheritdoc />
        public void AddComponentToLocalPlayer(IPlayer player, string component)
        {
            if (player is ServerPlayer)
            {
                var list = (player as ServerPlayer).RequiredComponents.ToList();
                if (!list.Contains(component))
                {
                    list.Add(component);
                    (player as ServerPlayer).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void AddComponentToPuppetPlayer(IPlayer player, string component)
        {
            if (player is ServerPlayer)
            {
                var list = (player as ServerPlayer).RequiredPuppetComponents.ToList();
                if (!list.Contains(component))
                {
                    list.Add(component);
                    (player as ServerPlayer).RequiredPuppetComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoveComponentFromLocalPlayer(IPlayer player, string component)
        {
            if (player is ServerPlayer)
            {
                var list = (player as ServerPlayer).RequiredComponents.ToList();
                if (list.Contains(component))
                {
                    list.Remove(component);
                    (player as ServerPlayer).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoteComponentFromPuppetPlayer(IPlayer player, string component)
        {
            if (player is ServerPlayer)
            {
                var list = (player as ServerPlayer).RequiredPuppetComponents.ToList();
                if (list.Contains(component))
                {
                    list.Remove(component);
                    (player as ServerPlayer).RequiredPuppetComponents = list.ToArray();
                }
            }
        }
    }
}