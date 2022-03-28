
using Godot;
using Framework.Utils;

namespace Framework.Game
{
    /// <summary>
    /// Required interface for game logic
    /// </summary>
    public interface IGameLogic : IBaseComponent
    {
        /// <summary>
        /// Service registry (which contains the service of the game logic)
        /// </summary>
        /// <value></value>
        public TypeDictonary<IService> Services { get; }

        /// <summary>
        /// The async map loader 
        /// </summary>
        /// <value></value>
        public AsyncLoader MapLoader { get; }
    }
}
