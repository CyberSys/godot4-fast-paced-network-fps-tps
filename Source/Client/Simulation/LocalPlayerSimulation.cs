using Shooter.Shared;
using System;
using Shooter.Client.Simulation.Components;
using Shooter.Shared.Network.Packages;
using Godot;
using System.Collections.Generic;

namespace Shooter.Client.Simulation
{
    public partial class LocalPlayerSimulation : PhysicsPlayerSimulation
    {
        public LocalPlayerSimulation() : base()
        {

        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event.IsActionReleased("camera"))
            {
                var camera = this.Components.Get<PlayerCameraComponent>();
                if (camera != null)
                {
                    if (camera.cameraMode == CameraMode.FPS)
                    {
                        camera.cameraMode = CameraMode.TPS;
                    }
                    else if (camera.cameraMode == CameraMode.TPS)
                    {
                        camera.cameraMode = CameraMode.FPS;
                    }
                }
            }

            @event.Dispose();
        }
    }
}
