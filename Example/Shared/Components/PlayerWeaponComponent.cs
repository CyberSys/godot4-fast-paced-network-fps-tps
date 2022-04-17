using System;
using Godot;
using Framework.Network;
using Framework;
using LiteNetLib.Utils;
using Framework.Physics;
using Framework.Utils;

namespace Shooter.Shared.Components
{
	public struct PlayerWeaponPackage : INetSerializable
	{
		public int WeaponIndex { get; set; }

		public void Serialize(NetDataWriter writer)
		{
			writer.Put(WeaponIndex);
		}

		/// <inheritdoc />
		public void Deserialize(NetDataReader reader)
		{
			WeaponIndex = reader.GetInt();
		}
	}

	public partial class PlayerWeaponComponent : Node3D, IChildNetworkSyncComponent<PlayerWeaponPackage>, IPlayerComponent
	{
		public IBaseComponent BaseComponent { get; set; }

		[Export]
		NodePath weaponSwayRotationPath;

		[Export]
		NodePath weaponIdlePath;

		[Export]
		NodePath weaponSwayPath;

		[Export]
		NodePath weaponFirePositionPath;

		[Export]
		float bobSpeed = 0.05f;

		[Export]
		float bobDistance = 0.0002f;

		[Export]
		public float swayAmount = 6f;

		[Export]
		public float maxSwayX = 4f;
		[Export]
		public float maxSwayY = 4f;

		[Export]
		public float swaySmoothing = 0.1f;

		[Export]
		public float swayResetSmoothing = 0.1f;

		private Vector2 mouseMove = Vector2.Zero;

		private float timer, waveSlice;

		[Export]
		public float idleSwayAmountA = 1f;

		[Export]
		public float idleSwayAmountB = 2f;

		[Export]
		public float idleSwayScale = 400f;

		[Export]
		public float idleSwayLerpSpeed = 14f;

		[Export]
		Vector3 RecoilRotation = new Vector3(10, 5f, 7f);

		[Export]
		Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.2f);

		[Export]
		public float positionRecoilSpeed = 8f;

		[Export]
		public float rotationRecoilSpeed = 8f;

		[Export]
		public float rotationCameraSpeed = 6f;

		[Export]
		public float rotationCameraReturnSpeed = 8f;

		[Export]
		Vector3 RecoilCamera = new Vector3(6f, 1f, 6f);

		private float swayTime;

		private Vector3 newWeaponRotation = Vector3.Zero;
		private Vector3 newWeaponPosition = Vector3.Zero;

		private Vector3 targetWeaponRotation = Vector3.Zero;
		private Vector3 targetWeaponRotationVelocity = Vector3.Zero;
		private Vector3 newWeaponRotationVelocity = Vector3.Zero;

		private Vector3 rotationRecoil = Vector3.Zero;
		private Vector3 positionRecoil = Vector3.Zero;
		private Vector3 cameraRecoil = Vector3.Zero;
		private Vector3 Rot = Vector3.Zero;
		private Vector3 RotCamera = Vector3.Zero;

		[Export]
		public float fireRate = 0.2f;

		[Export]
		public bool requiredFire = true;

		private float fireRateTimer = 0f;
		private bool previousPressedState = false;

		public override void _EnterTree()
		{
			base._EnterTree();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
			{
				mouseMove = new Vector2((@event as InputEventMouseMotion).Relative.x, (@event as InputEventMouseMotion).Relative.y);
			}

			@event.Dispose();
		}

		public override void _Process(float delta)
		{
			base._Process(delta);

			var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
			var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

			if (camera != null)
			{
				// apply camera transform
				this.GlobalTransform = camera.GlobalTransform;
				this.HandleRecoil(camera, delta);
			}

			// swaying walk (only client sided)
			if (this.BaseComponent is Framework.Game.Client.LocalPlayer)
			{
				this.SwayWalk(delta);
				this.HandleSwayLook(delta);
				this.HandleIdleSway(delta);
			}
		}

		private void HandleRecoil(PlayerCameraComponent camera, float delta)
		{
			if (camera != null)
			{
				// fire recoil
				rotationRecoil = rotationRecoil.Lerp(Vector3.Zero, rotationRecoilSpeed * delta);
				positionRecoil = positionRecoil.Lerp(Vector3.Zero, positionRecoilSpeed * delta);
				Rot = Rot.Lerp(rotationRecoil, rotationRecoilSpeed * delta);

				var position = this.GetNode<Node3D>(weaponFirePositionPath).Position;
				position = position.Lerp(positionRecoil, positionRecoilSpeed * delta);
				var rotationTransform = this.GetNode<Node3D>(weaponFirePositionPath).Rotation;
				rotationTransform = Rot;
				this.GetNode<Node3D>(weaponFirePositionPath).Position = position;
				this.GetNode<Node3D>(weaponFirePositionPath).Rotation = rotationTransform;

				// fire recoil camera
				cameraRecoil = cameraRecoil.Lerp(Vector3.Zero, rotationCameraReturnSpeed * delta);
				RotCamera = RotCamera.Lerp(cameraRecoil, rotationCameraSpeed * delta);

				var cameraRotation = camera.Rotation;
				cameraRotation += RotCamera;
				camera.Rotation = cameraRotation;
			}
		}

		private void SwayWalk(float delta)
		{
			var input = this.BaseComponent.Components.Get<PlayerInputComponent>();
			var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
			if (input != null && body != null)
			{
				var currentInput = input.GetPlayerInput();

				var horizontal = currentInput.GetInput("Right") ? 1f : currentInput.GetInput("Left") ? -1f : 0f;
				var vertical = currentInput.GetInput("Back") ? 1f : currentInput.GetInput("Forward") ? -1f : 0f;
				var factor = body.MovementProcessor.GetMovementSpeedFactor();

				if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
				{
					timer = 0.0f;
				}
				else
				{
					waveSlice = Mathf.Sin(timer);
					timer = timer + (bobSpeed * factor);
					if (timer > Mathf.Pi * 2)
					{
						timer = timer - (Mathf.Pi * 2);
					}
				}

				var swayPos = this.GetNode<Node3D>(weaponSwayPath).Position;
				if (waveSlice != 0)
				{
					float translateChange = waveSlice * (bobDistance * (factor));
					float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
					totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
					translateChange = totalAxes * translateChange;

					swayPos.y += translateChange;
					swayPos.x += translateChange * 2;
				}
				else
				{
					swayPos = Vector3.Zero;
				}

				this.GetNode<Node3D>(weaponSwayPath).Position = swayPos;
			}
		}

		private void HandleSwayLook(float delta)
		{
			// swaying on look
			targetWeaponRotation.y += mouseMove.x * swayAmount * delta;
			targetWeaponRotation.x += -mouseMove.y * swayAmount * delta;

			targetWeaponRotation.x = Math.Clamp(targetWeaponRotation.x, -maxSwayX, maxSwayX);
			targetWeaponRotation.y = Math.Clamp(targetWeaponRotation.y, -maxSwayY, maxSwayY);

			targetWeaponRotation = VectorExtension.SmoothDamp(targetWeaponRotation, Vector3.Zero, ref targetWeaponRotationVelocity, swayResetSmoothing, delta);
			newWeaponRotation = VectorExtension.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, swaySmoothing, delta);

			var weaponRot = this.GetNode<Node3D>(weaponSwayRotationPath).Rotation;
			weaponRot = new Vector3(Mathf.Deg2Rad(newWeaponRotation.x), Mathf.Deg2Rad(newWeaponRotation.y), Mathf.Deg2Rad(newWeaponRotation.z));
			this.GetNode<Node3D>(weaponSwayRotationPath).Rotation = weaponRot;
		}

		private void HandleIdleSway(float delta)
		{
			// idle sway
			var targetPosition = LissajousCurve(swayTime, idleSwayAmountA, idleSwayAmountB) / idleSwayScale;
			newWeaponPosition = newWeaponPosition.Lerp(targetPosition, delta * idleSwayLerpSpeed);
			swayTime += delta;
			if (swayTime > 6.3f)
			{
				swayTime = 0;
			}

			var transformIdle = this.GetNode<Node3D>(weaponIdlePath).Transform;
			transformIdle.origin = newWeaponPosition;
			this.GetNode<Node3D>(weaponIdlePath).Transform = transformIdle;
		}

		public void DoShoot(float delta)
		{
			bool pressed = (this.BaseComponent as PhysicsPlayer).LastInput.GetInput("Fire");

			if (!requiredFire)
			{
				if (fireRateTimer <= 0f && pressed)
				{
					this.AddShot();
					this.fireRateTimer = this.fireRate;
				}
				else
				{
					fireRateTimer -= delta;
				}
			}
			else
			{
				if (previousPressedState == false && pressed && fireRateTimer <= 0f)
				{
					this.AddShot();
					this.fireRateTimer = this.fireRate;
				}
				else
				{
					fireRateTimer -= delta;
				}
				previousPressedState = pressed;
			}
		}

		private Vector3 LissajousCurve(float Time, float A, float B)
		{
			return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.Pi), 0);
		}

		public void Tick(float delta)
		{
			this.DoShoot(delta);
		}

		private void AddShot()
		{
			var rd = new RandomNumberGenerator();
			rd.Randomize();
			var rotY = rd.RandfRange(-RecoilRotation.y, RecoilRotation.y);
			rd.Randomize();
			var rotZ = rd.RandfRange(-RecoilRotation.z, RecoilRotation.z);
			rd.Randomize();
			var kickBackX = rd.RandfRange(-RecoilKickBack.x, RecoilKickBack.x);
			rd.Randomize();
			var kickBackY = rd.RandfRange(-RecoilKickBack.y, RecoilKickBack.y);

			rotationRecoil += new Vector3(Mathf.Deg2Rad(RecoilRotation.x), Mathf.Deg2Rad(rotY), Mathf.Deg2Rad(rotZ));
			positionRecoil += new Vector3(kickBackX, kickBackY, RecoilKickBack.z);
			rd.Randomize();

			var cameraRecoilY = rd.RandfRange(-RecoilCamera.y, RecoilCamera.y);
			rd.Randomize();
			var cameraRecoilZ = rd.RandfRange(-RecoilCamera.z, RecoilCamera.z);

			cameraRecoil += new Vector3(Mathf.Deg2Rad(RecoilCamera.x), Mathf.Deg2Rad(cameraRecoilY), Mathf.Deg2Rad(cameraRecoilZ));
		}

		public void ApplyNetworkState(PlayerWeaponPackage package)
		{

		}

		public PlayerWeaponPackage GetNetworkState()
		{
			return new PlayerWeaponPackage
			{

			};
		}
	}
}
