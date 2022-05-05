using Framework.Game;
using System.Collections.Generic;
using System.Linq;
using Framework.Game.Server;
using Framework;
using Shooter.Shared.Components;
using Framework.Input;

namespace Shooter.Server
{
    public class DeathmatchGameRule : GameRule
    {
        public Queue<INetworkCharacter> playersWaitingForSlot = new Queue<INetworkCharacter>();

        public override void OnNewPlayerJoined(INetworkCharacter player)
        {
            var spawnpoints = this.GameWorld.NetworkLevel.GetFreeSpawnPoints();
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

        private void AddPlayerWithSlot(SpawnPoint spawnPoint, INetworkCharacter player)
        {
            var origin = spawnPoint.GlobalTransform.origin;

            Logger.LogDebug(this, "Player was joined to " + origin.ToString());

            this.AddComponentToServerPlayer<PlayerAnimationComponent>(player);
            this.AddComponentToServerPlayer<PlayerCameraComponent>(player);
            this.AddComponentToServerPlayer<PlayerFootstepComponent>(player);
            this.AddComponentToServerPlayer<PlayerWeaponComponent>(player);
            this.AddComponentToServerPlayer<NetworkInput>(player);

            this.AddComponentToLocalPlayer<PlayerAnimationComponent>(player);
            this.AddComponentToLocalPlayer<NetworkInput>(player);
            this.AddComponentToLocalPlayer<PlayerCameraComponent>(player);
            this.AddComponentToLocalPlayer<PlayerFootstepComponent>(player);
            this.AddComponentToLocalPlayer<PlayerWeaponComponent>(player);

            this.AddComponentToPuppetPlayer<PlayerWeaponComponent>(player);
            this.AddComponentToPuppetPlayer<PlayerAnimationComponent>(player);
            this.AddComponentToPuppetPlayer<PlayerFootstepComponent>(player);

            player.DoTeleport(origin);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            if (playersWaitingForSlot.Count > 0)
            {
                var spawnpoints = this.GameWorld.NetworkLevel.GetFreeSpawnPoints();
                if (spawnpoints.Length > 0)
                {
                    var player = playersWaitingForSlot.Dequeue();
                    this.AddPlayerWithSlot(spawnpoints.First(), player);
                }
            }
        }
    }
}