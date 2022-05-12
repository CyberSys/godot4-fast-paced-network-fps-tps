using System.Linq;
using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;
using Godot;
using Framework.Physics;
using System;
using Framework.Network.Commands;
using Framework.Network;
using Framework;
namespace Shooter.Shared
{
    public partial class MyPlayer : NetworkCharacter
    {
        [Export]
        public NodePath TPSPath { get; set; }

        [Export]
        public NodePath FPSPath { get; set; }

        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToPuppet)]
        public float AnimationMoveSpeed = 0f;

        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToPuppet)]
        public Vector2 AnimationBlendPosition = Vector2.Zero;

        /// <summary>
        /// Custom net vars
        /// </summary>
        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet)]
        public float HP = 0f;

        public override void _EnterTree()
        {
            base._EnterTree();
            this.GetNode<Node3D>(this.FPSPath).Visible = false;
        }

        public override PlayerState Interpolate(float theta, PlayerState lastState, PlayerState nextState)
        {
            var currentInterpolation = base.Interpolate(theta, lastState, nextState);

            var aPos = lastState.GetVar<Vector2>(this, "AnimationBlendPosition");
            var bPos = nextState.GetVar<Vector2>(this, "AnimationBlendPosition");

            var aSpeedPos = lastState.GetVar<float>(this, "AnimationMoveSpeed");
            var bSpeedPos = nextState.GetVar<float>(this, "AnimationMoveSpeed");

            currentInterpolation.SetVar(this, "AnimationBlendPosition", aPos.Slerp(bPos, theta));
            currentInterpolation.SetVar(this, "AnimationMoveSpeed", Mathf.Lerp(aSpeedPos, bSpeedPos, theta));

            return currentInterpolation;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (!this.IsPuppet())
            {
                this.AnimationMoveSpeed = this.MovementProcessor.GetMovementSpeedFactor();
                this.AnimationBlendPosition = new Vector2(this.MovementProcessor.ForwardBackwardAxis,
                this.MovementProcessor.LeftRightAxis);
            }

            if (this.IsLocal())
            {
                var camera = this.Components.Get<CharacterCamera>();
                if (camera == null)
                    return;

                var shadowMode = GeometryInstance3D.ShadowCastingSetting.On;
                if (camera != null && camera.IsInsideTree() && camera.Mode == CameraMode.FPS)
                {
                    shadowMode = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
                    this.GetNode<Node3D>(this.FPSPath).Visible = true;
                }
                if (camera != null && camera.IsInsideTree() && camera.Mode == CameraMode.TPS)
                {
                    this.GetNode<Node3D>(this.FPSPath).Visible = false;
                }

                this.SetShadowModes(this.GetNode(TPSPath), shadowMode);
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