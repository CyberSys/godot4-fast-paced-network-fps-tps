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
    public class GameRule : IGameRule
    {
        public ServerWorld GameWorld { get; private set; }

        public GameRule(ServerWorld gameWorld)
        {
            GameWorld = gameWorld;
        }

        public void AddPlayerComponent(IPlayer player, string name)
        {

        }

        public virtual void OnNewPlayerJoined(IPlayer player)
        {

        }

        public virtual void OnPlayerRejoined(IPlayer player)
        {

        }

        public virtual void OnPlayerLeaveTemporary(IPlayer player)
        {

        }
        public virtual void OnPlayerLeave(IPlayer player)
        {

        }

        public virtual void Tick(float delta)
        {

        }

        public void AddComponentToPlayer(IPlayer player, string component)
        {
            if (player is Player)
            {
                var components = (player as Player).AvaiablePlayerComponents;
                if (components.ContainsKey(component) && player is Player)
                {
                    var componentRegistry = components[component];
                    Logger.LogDebug(this, "Found registry component: " + componentRegistry.NodeType.ToString() + " => " + componentRegistry.ResourcePath);

                    var playerInstance = player as Player;
                    var playerComponents = playerInstance.Components;

                    var method = typeof(ComponentRegistry).GetMethods().Single(
                m =>
                    m.Name == "AddComponent" &&
                    m.GetGenericArguments().Length == 1 &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(string));

                    MethodInfo generic = method.MakeGenericMethod(componentRegistry.NodeType);
                    var createdObject = generic.Invoke(player.Components, new object[]
                    {
                    componentRegistry.ResourcePath
                    });

                    if (createdObject is IMoveable && player is PhysicsPlayer)
                    {
                        (player as PhysicsPlayer).Body = (IMoveable)createdObject;
                    }
                }
            }
        }

        public void AddRemoteComponentToPlayer(IPlayer player, string component)
        {

        }
    }

    public interface IGameRule
    {
        public ServerWorld GameWorld { get; }

        public void OnNewPlayerJoined(IPlayer player);
        public void OnPlayerRejoined(IPlayer player);

        public void OnPlayerLeaveTemporary(IPlayer player);
        public void OnPlayerLeave(IPlayer player);

        public void Tick(float delta);
    }
}