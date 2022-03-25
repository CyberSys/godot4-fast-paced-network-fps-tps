using System.Collections.Generic;
using Godot;
using Shooter.Shared.Network.Packages;
namespace Shooter.Shared
{
    public interface IWorld
    {
        public void Init(uint initalWorldTick);
        public void Destroy();
        public T AddPlayerSimulation<T>(int id) where T : PlayerSimulation;
        public void RemovePlayerSimulation(int id);
        public PlayerSimulation GetPlayerSimulation(int id);
        public uint WorldTick { get; }
        public GameLevel Level { get; }

    }
}
