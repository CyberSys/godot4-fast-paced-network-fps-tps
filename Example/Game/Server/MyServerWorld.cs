using Framework.Game.Server;

namespace Shooter.Server
{
    public partial class MyServerWorld : NetworkServerWorld
    {
        public override void OnPlayerConnected(short clientId)
        {
            this.AddPlayer(clientId, "res://Game/Assets/Player/MyNetworkCharacter.tscn"
                , new string[] {
                    "res://Game/Shared/MyPlayer.cs",
                    "res://Framework/Game/CharacterCamera.cs",
                    "res://Framework/Game/NetworkInput.cs",
                    "res://Game/Shared/Components/FPSWeaponAnimator.cs",
                    "res://Game/Shared/Components/FPSWeaponHandler.cs",
                    "res://Game/Shared/Components/PlayerFootstepComponent.cs",
                    "res://Game/Shared/Components/PlayerAnimationComponent.cs"
                });
        }
    }
}