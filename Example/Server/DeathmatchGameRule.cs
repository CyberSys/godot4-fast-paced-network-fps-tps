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

            this.AddComponentToPlayer(player, "body");
            this.AddComponentToPlayer(player, "camera");
            this.AddComponentToPlayer(player, "footsteps");

            this.AddRemoteComponentToPlayer(player, "body");
            this.AddRemoteComponentToPlayer(player, "camera");
            this.AddRemoteComponentToPlayer(player, "input");
            this.AddRemoteComponentToPlayer(player, "footsteps");


            this.AddPuppetComponentToPlayer(player, "body");
            this.AddPuppetComponentToPlayer(player, "footsteps");

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