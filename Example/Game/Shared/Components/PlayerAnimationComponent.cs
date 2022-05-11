using Framework;
using Framework.Network;
using Framework.Physics;
using Godot;
using Framework.Game;
using Framework.Input;

namespace Shooter.Shared.Components
{
    public partial class PlayerAnimationComponent : Node, IPlayerComponent
    {
        public short NetworkId { get; set; } = 4;

        [Export]
        public bool IsEnabled { get; set; } = false;

        [Export]
        public NodePath AnimationTreePath { get; set; }

        [Export]
        public NodePath InputPath { get; set; }

        [Export]
        public NodePath CameraPath { get; set; }

        private AnimationTree AnimTree { get; set; }

        private CharacterCamera Camera { get; set; }

        public Framework.Game.NetworkCharacter BaseComponent { get; set; }
        public Framework.Game.NetworkInput Input { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
            this.AnimTree = this.GetNode<AnimationTree>(this.AnimationTreePath);
            this.Camera = this.GetNode<CharacterCamera>(this.CameraPath);
            this.Input = this.GetNode<Framework.Game.NetworkInput>(this.InputPath);
            this.AnimTree.Active = true;
        }
        public void Tick(float delta)
        {
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            var factor = (this.BaseComponent as MyPlayer).AnimationMoveSpeed;
            var blendPosition = (this.BaseComponent as MyPlayer).AnimationBlendPosition;

            if (!this.IsPuppet())
            {
                if (Input == null || Camera == null)
                {
                    return;
                }

                var rotX = Mathf.Clamp(Mathf.Rad2Deg(Input.LastInput.ViewDirection.y), -90f, 90f) / 90f;
                var rotY = Mathf.Clamp(Mathf.Rad2Deg(Input.LastInput.ViewDirection.x), -90f, 90f) / 90f;

                this.AnimTree.Set("parameters/ShoulderMovement/add_amount", rotX * -1);
                this.AnimTree.Set("parameters/ShoulderMiovementUp/add_amount", rotY);
            }

            this.AnimTree.Set("parameters/MoveVec/blend_position", blendPosition);
            this.AnimTree.Set("parameters/MoveSpeed/scale", 1f + factor);
            this.AnimTree.Set("parameters/MoveState/current", factor > 0 ? 1 : 0);
        }

    }
}
