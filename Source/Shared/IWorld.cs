using System.Collections.Generic;
using Godot;
using Shooter.Shared.Network.Packages;
namespace Shooter.Shared
{
    public interface IWorld
    {
        public void Init(Dictionary<string, string> serverVars, uint initalWorldTick);
        public void Destroy();
        public T AddPlayerSimulation<T>(int id) where T : NetworkPlayerSimulation;
        public void RemovePlayerSimulation(NetworkPlayerSimulation simulation);
        public uint WorldTick { get; }
        public GameLevel Level { get; }

        public Dictionary<int, IPlayer> Players { get; }

    }
}
