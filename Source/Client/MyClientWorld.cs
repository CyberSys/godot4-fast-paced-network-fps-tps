using Framework.Game.Client;
using Framework.Game;
using Shooter.Share.Components;

namespace Shooter.Client
{
    public partial class MyClientWorld : ClientWorld
    {
        /*
        public override void OnClientPlayerCreation(IPlayer p)
        {
            if (p is PuppetPlayer)
            {
                // var puppet = p as PuppetPlayer;
                //  puppet.Simulation.Body = puppet.Simulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
            }

            else if (p is LocalPlayer)
            {
                //   var local = p as LocalPlayer;
                //   var bodyComponent = local.Simulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                //   local.Simulation.Body = bodyComponent;

                //add component
                //   local.Simulation.Components.AddComponent<PlayerCameraComponent>();
                //   var inputComponent = local.Simulation.Components.AddComponent<PlayerInputComponent>();
                // local.Simulation.Inputable = inputComponent;
            }
        }
        */
    }
}