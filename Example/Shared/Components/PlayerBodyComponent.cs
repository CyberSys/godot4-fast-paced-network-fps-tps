using Framework;
using Framework.Network;
using Framework.Physics;
using Godot;
using System;
using Framework.Network.Commands;
using Framework.Game;
using Framework.Physics.Commands;

namespace Shooter.Shared.Components
{
	public partial class PlayerBodyComponent : NetworkPlayerBody
	{
		[Export]
		public NodePath AnimationTreePath { get; set; }

		private AnimationTree AnimTree { get; set; }

		public override void _EnterTree()
		{
			base._EnterTree();
			this.AnimTree = this.GetNode<AnimationTree>(this.AnimationTreePath);
			this.AnimTree.Active = true;
		}

		public override void _Process(float delta)
		{
			base._Process(delta);

			if (this.IsLocal())
			{
				var player = this.BaseComponent as PhysicsPlayer;

				var factor = player.MovementProcessor.GetMovementSpeedFactor();
				var direction = player.LastInput.ViewDirection;

				var state = (System.Int64)this.AnimTree.Get("parameters/MoveState/current");
				this.AnimTree.Set("parameters/MoveSpeed/scale", 1f + factor);
				this.AnimTree.Set("parameters/MoveVec/blend_position", new Vector2(
					player.MovementProcessor.ForwardBackwardAxis,
				player.MovementProcessor.LeftRightAxis));

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
