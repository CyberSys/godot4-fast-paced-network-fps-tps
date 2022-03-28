using System.Linq;
using Framework.Physics;
using Framework.Game.Server;
using System.Reflection;

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
        public void AddComponentToPlayer(IPlayer player, string component)
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

            if (createdObject is IMoveable && player is PhysicsPlayer)
            {
                (player as PhysicsPlayer).Body = (IMoveable)createdObject;
            }
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

            if (createdObject is IMoveable && player is PhysicsPlayer)
            {
                (player as PhysicsPlayer).Body = (IMoveable)createdObject;
            }
        }

        /// <inheritdoc />
        public void AddRemoteComponentToPlayer(IPlayer player, string component)
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
        public void AddPuppetComponentToPlayer(IPlayer player, string component)
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
        public void RemoveRemoteComponentFromPlayer(IPlayer player, string component)
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
        public void RemoveRemoteComponentFromPuppet(IPlayer player, string component)
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