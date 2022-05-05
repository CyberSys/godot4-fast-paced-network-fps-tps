using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;
using Godot;
using Framework.Physics;

namespace Shooter.Shared
{
    public partial class MyPlayer : NetworkCharacter
    {
        [Export]
        NodePath characterPath;

        public MyPlayer() : base()
        {
            this.AddAvaiableComponent<PlayerCameraComponent>();
            this.AddAvaiableComponent<PlayerFootstepComponent>();
            this.AddAvaiableComponent<Framework.Game.NetworkInput>();
            this.AddAvaiableComponent<PlayerWeaponComponent>("res://Game/Assets/Weapons/WeaponHolder.tscn",
            "res://Game/Shared/Components/PlayerWeaponComponent.cs");
        }
        public override void _Process(float delta)
        {
            base._Process(delta);

            if (this.IsLocal())
            {
                var shadowMode = GeometryInstance3D.ShadowCastingSetting.On;
                var camera = this.Components.Get<PlayerCameraComponent>();

                if (camera != null && camera.IsInsideTree() && camera.Mode == CameraMode.FPS)
                {
                    shadowMode = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
                }

                this.SetShadowModes(this.GetNode(characterPath), shadowMode);
            }
        }
        private void SetShadowModes(Node t, GeometryInstance3D.ShadowCastingSetting mode)
        {
            if (t is MeshInstance3D)
            {
                (t as MeshInstance3D).CastShadow = mode;
            }

            foreach (var element in t.GetChildren())
            {
                if (element is Node)
                {
                    SetShadowModes(element as Node, mode);
                }
            }
        }

    }
}