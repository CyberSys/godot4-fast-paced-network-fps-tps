using System.Security.AccessControl;
using System.Numerics;
using Framework.Game;
using System.Collections.Generic;
using System;
using System.Linq;
using Framework.Game.Server;
using Framework;
namespace Shooter.Server
{
    public class DeathmatchGameRule : GameRule
    {
        public Queue<IPlayer> playersWaitingForSlot = new Queue<IPlayer>();

        public DeathmatchGameRule(MyServerWorld gameWorld) : base(gameWorld)
        {

        }

        public override void OnNewPlayerJoined(IPlayer player)
        {
            var spawnpoints = this.GameWorld.Level.GetFreeSpawnPoints();
            if (spawnpoints.Length > 0)
            {
                this.AddPlayerWithSlot(spawnpoints.First(), player);
            }
            else
            {
                Logger.LogDebug(this, "Cant find free slot for player " + player.Id);
                this.playersWaitingForSlot.Enqueue(player);
            }
        }

        private void AddPlayerWithSlot(SpawnPoint spawnPoint, IPlayer player)
        {
            var origin = spawnPoint.GlobalTransform.origin;

            Logger.LogDebug(this, "Player was joined to " + origin.ToString());

            var serverPlayer = player as ServerPlayer;

            this.AddComponentToServerPlayer(player, "body");
            this.AddComponentToServerPlayer(player, "camera");
            this.AddComponentToServerPlayer(player, "footsteps");

            this.AddComponentToLocalPlayer(player, "body");
            this.AddComponentToLocalPlayer(player, "camera");
            this.AddComponentToLocalPlayer(player, "input");
            this.AddComponentToLocalPlayer(player, "footsteps");

            this.AddComponentToPuppetPlayer(player, "body");
            this.AddComponentToPuppetPlayer(player, "footsteps");

            serverPlayer.DoTeleport(origin);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (playersWaitingForSlot.Count > 0)
            {
                var spawnpoints = this.GameWorld.Level.GetFreeSpawnPoints();
                if (spawnpoints.Length > 0)
                {
                    var player = playersWaitingForSlot.Dequeue();
                    this.AddPlayerWithSlot(spawnpoints.First(), player);
                }
            }
        }
    }
}