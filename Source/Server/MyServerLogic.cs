using Framework.Game.Server;
using Framework.Game;
using Shooter.Share.Components;
using Godot;

namespace Shooter.Server
{
    public partial class MyServerLogic : ServerLogic<MyServerWorld>
    {
        [Export]
        /// <summary>
        /// The default map name which is loaded after startup
        /// </summary>
        public string DefaultMapName = "res://Assets/Maps/Test.tscn";

        public override void OnServerStarted()
        {
            this.LoadWorld(DefaultMapName, 0);
        }

        public override void _EnterTree()
        {
            base._EnterTree();
        }

        public override void AfterMapInstance()
        {
            var deathmatch = new DeathmatchGameRule(this.currentWorld as MyServerWorld);
            (this.currentWorld as MyServerWorld).ActiveGameRule = deathmatch;
        }
    }
}