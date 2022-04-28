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
using Framework.Network;

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

        /// <summary>
        /// Trigger when an player got an hit
        /// </summary>
        public virtual void OnHit(RayCastHit player)
        {

        }

        /// <inheritdoc />
        public void AddComponentToServerPlayer<T>(IPlayer player) where T : IChildComponent
        {
            if (player is Player)
            {
                var compIndex = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                if (compIndex > -1)
                {
                    var component = player.AvaiablePlayerComponents[compIndex];
                    if (player is Player)
                    {
                        Logger.LogDebug(this, "Found registry component: " + component.NodeType.ToString() + " => " + component.ResourcePath);

                        var playerInstance = player as Player;
                        var playerComponents = playerInstance.Components;

                        if (component.ResourcePath != null)
                        {
                            this.CreateComponentWithResource(player, component);
                        }
                        else
                        {
                            this.CreateComponent(player, component);
                        }
                    }
                }
            }
        }

        internal void CreateComponent(IPlayer player, AssignedComponent registeredComonent)
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

            //attach the body to the local player
            if (createdObject is NetworkPlayerBody && player is NetworkPlayer)
            {
                (player as NetworkPlayer).Body = createdObject as NetworkPlayerBody;
            }

            //attach the camera to the local player
            if (createdObject is PhysicsPlayerCamera && player is PhysicsPlayer)
            {
                (player as PhysicsPlayer).Camera = createdObject as PhysicsPlayerCamera;
            }
        }

        internal void CreateComponentWithResource(IPlayer player, AssignedComponent registeredComonent)
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

            //attach the body to the server player
            if (createdObject is NetworkPlayerBody && player is NetworkPlayer)
            {
                (player as NetworkPlayer).Body = createdObject as NetworkPlayerBody;
            }

            //attach the camera to the server player
            if (createdObject is PhysicsPlayerCamera && player is PhysicsPlayer)
            {
                (player as PhysicsPlayer).Camera = createdObject as PhysicsPlayerCamera;
            }
        }

        /// <inheritdoc />
        public void AddComponentToLocalPlayer<T>(IPlayer player) where T : IChildComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                Logger.LogDebug(this, "Found index " + index);
                var list = (player as ServerPlayer).RequiredComponents.ToList();
                if (index > -1)
                {
                    list.Add(index);
                    (player as ServerPlayer).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void AddComponentToPuppetPlayer<T>(IPlayer player) where T : IChildComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));
                var list = (player as ServerPlayer).RequiredPuppetComponents.ToList();
                if (index > -1)
                {
                    list.Add(index);
                    (player as ServerPlayer).RequiredPuppetComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoveComponentFromLocalPlayer<T>(IPlayer player) where T : IChildComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));

                var list = (player as ServerPlayer).RequiredComponents.ToList();
                if (index > -1)
                {
                    list.Remove(index);
                    (player as ServerPlayer).RequiredComponents = list.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public void RemoteComponentFromPuppetPlayer<T>(IPlayer player) where T : IChildComponent
        {
            if (player.IsServer())
            {
                var index = player.AvaiablePlayerComponents.FindIndex(df => df.NodeType == typeof(T));

                var list = (player as ServerPlayer).RequiredPuppetComponents.ToList();
                if (index > -1)
                {
                    list.Remove(index);
                    (player as ServerPlayer).RequiredPuppetComponents = list.ToArray();
                }
            }
        }
    }
}