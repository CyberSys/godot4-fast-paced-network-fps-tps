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
        [Export]
        public NodePath AnimationTreePath { get; set; }

        private AnimationTree AnimTree { get; set; }

        public Framework.Game.NetworkCharacter BaseComponent { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
            this.AnimTree = this.GetNode<AnimationTree>(this.AnimationTreePath);
            this.AnimTree.Active = true;
        }
        public void Tick(float delta)
        {
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (this.IsLocal())
            {
                var inputComponent = this.BaseComponent.Components.Get<NetworkInput>();
                if (inputComponent == null)
                {
                    return;
                }

                var factor = this.BaseComponent.MovementProcessor.GetMovementSpeedFactor();
                var direction = inputComponent.LastInput.ViewDirection;

                var state = (System.Int64)this.AnimTree.Get("parameters/MoveState/current");
                this.AnimTree.Set("parameters/MoveSpeed/scale", 1f + factor);
                this.AnimTree.Set("parameters/MoveVec/blend_position", new Vector2(
                    this.BaseComponent.MovementProcessor.ForwardBackwardAxis,
                this.BaseComponent.MovementProcessor.LeftRightAxis));

                if (direction != new Quaternion(0, 0, 0, 0))
                {
                    var euler = direction.GetEuler();
                    var rotX = Mathf.Clamp(Mathf.Rad2Deg(euler.y), -90f, 90f) / 90f;

                    var rotY = Mathf.Clamp(Mathf.Rad2Deg(euler.x), -90f, 90f) / 90f;

                    this.AnimTree.Set("parameters/ShoulderMovement/add_amount", rotX * -1);
                    this.AnimTree.Set("parameters/ShoulderMiovementUp/add_amount", rotY);
                }

                this.AnimTree.Set("parameters/MoveState/current", factor > 0 ? 1 : 0);
            }
            else
            {
                //interpolate animations /todo
            }
        }

    }
}
