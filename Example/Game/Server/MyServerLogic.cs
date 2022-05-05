using Framework.Game.Server;
using Framework.Game;
using Godot;

namespace Shooter.Server
{
    public partial class MyServerLogic : NetworkServerLogic
    {
        [Export]
        /// <summary>
        /// The default map name which is loaded after startup
        /// </summary> 
        public string DefaultMapName = "res://Game/Assets/Maps/Test.tscn";

        public override void OnServerStarted()
        {
            this.LoadWorld(DefaultMapName, "res://Game/Shared/MyGameLevel.cs", 0);
        }

        public override NetworkServerWorld CreateWorld()
        {
            return GD.Load<PackedScene>("res://Game/Server/MyServerWorld.tscn").Instantiate<MyServerWorld>();
        }

        public override void AfterMapLoaded()
        {
            (this.currentWorld as NetworkServerWorld).SetGameRule<DeathmatchGameRule>("deathmatch");
        }
    }
}