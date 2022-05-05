using Framework.Game.Server;

namespace Shooter.Server
{
    public partial class MyServerWorld : NetworkServerWorld
    {
        public override void OnPlayerConnected(int clientId)
        {
            this.AddPlayer(clientId, "res://Game/Assets/Player/MyNetworkCharacter.tscn"
                , "res://Game/Shared/MyPlayer.cs");
        }
    }
}