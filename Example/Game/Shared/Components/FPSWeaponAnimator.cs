using System;
using Godot;
using Framework.Network;
using Framework;
using LiteNetLib.Utils;
using Framework.Physics;
using Framework.Utils;
using Framework.Game.Client;
using Framework.Game;
using Framework.Input;
namespace Shooter.Shared.Components
{
    [Tool]
    public partial class FPSWeaponAnimator : Node, IPlayerComponent
    {
        public short NetworkId { get; set; } = 6;

        [Export]
        public bool IsEnabled { get; set; } = false;

        public Framework.Game.NetworkCharacter BaseComponent { get; set; }

        [Export]
        public NodePath cameraPath { get; set; }

        [Export]
        public NodePath inputPath { get; set; }


        [Export]
        public NodePath weaponSwayPath { get; set; }

        [Export]
        public AudioStreamSample ShootSound { get; set; }

        [Export]
        public NodePath ShootSoundPlayerPath { get; set; }

        private AudioStreamPlayer3D ShootSoundPlayer { get; set; }

        /// Walk boobing
        public bool SwayWalkBobEnabled = true;
        public float SwayWalkBobSpeed = 12f;
        public float SwayWalkBobDistance = 0.02f;

        // Sway move
        public bool SwayLookMoveEnabled = true;
        public float SwayLookMoveAmount = 6f;
        public float SwayLookMoveSmoothing = 0.1f;
        public float SwayLookMoveResetSmoothing = 0.1f;
        public Vector2 SwayLookMoveMax = new Vector2(4f, 4f);

        private Vector2 mouseMove = Vector2.Zero;

        private float timer, waveSlice;

        /// Idle sway
        public bool IdleSwayEnabled = false;
        public float IdleSwayAmountA = 1f;
        public float IdleSwayAmountB = 2f;
        public float IdleSwayScale = 400f;
        public float IdleSwayLerpSpeed = 14f;

        /// Kickback
        public bool RecoilKickBackEnabled = false;

        private bool _ShooTest = false;
        public bool ShootTest
        {
            get
            {
                return _ShooTest;
            }
            set
            {
                _ShooTest = value;
            }
        }

        Vector3 RecoilKickBackRotation = new Vector3(10, 5f, 7f);

        Vector3 RecoilKickBackPosition = new Vector3(0.015f, 0f, 0.1f);
        public float RecoilKickBackPositionSpeed = 12f;
        public float RecoilKickBackRotationSpeed = 10f;

        /// Camera Recoil
        public float CameraRecoilRotationSpeed = 6f;
        public float CameraRecoilRotationReturnSpeed = 8f;
        Vector3 CameraRecoilPosition = new Vector3(6f, 1f, 6f);

        private float swayTime;

        private Vector3 newWeaponRotation = Vector3.Zero;
        private Vector3 newWeaponPosition = Vector3.Zero;

        private Vector3 targetWeaponRotation = Vector3.Zero;
        private Vector3 targetWeaponRotationVelocity = Vector3.Zero;
        private Vector3 newWeaponRotationVelocity = Vector3.Zero;

        private Vector3 rotationRecoil = Vector3.Zero;
        private Vector3 positionRecoil = Vector3.Zero;
        private Vector3 cameraRecoil = Vector3.Zero;
        private Vector3 RotCamera = Vector3.Zero;

        [Export]
        public float fireRate = 0.1f;

        [Export]
        public bool requiredFire = true;

        private float fireRateTimer = 0f;
        private bool previousPressedState = false;

        private Node3D swayNodePos;

        private CharacterCamera camera;

        private NetworkInput input;

        public override void _EnterTree()
        {
            base._EnterTree();

            swayNodePos = this.GetNodeOrNull<Node3D>(weaponSwayPath);
            camera = this.GetNodeOrNull<CharacterCamera>(cameraPath);
            input = this.GetNodeOrNull<NetworkInput>(inputPath);

            ShootSoundPlayer = this.GetNodeOrNull<AudioStreamPlayer3D>(ShootSoundPlayerPath);
        }

        public override void _Input(InputEvent @event)
        {
            if (this.BaseComponent.IsLocal() && !Engine.IsEditorHint())
            {
                if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
                {
                    mouseMove = new Vector2((@event as InputEventMouseMotion).Relative.x, (@event as InputEventMouseMotion).Relative.y);
                }
            }

            @event.Dispose();
        }

        public override void _Process(float delta)
        {
            if (swayNodePos == null)
            {
                return;
            }

            swayNodePos.Position = Vector3.Zero;
            swayNodePos.Rotation = Vector3.Zero;

            if (!Engine.IsEditorHint())
            {
                if (this.camera == null || this.IsEnabled == false)
                    return;

                // swaying walk (only client sided)
                if (this.IsLocal())
                {
                    this.HandleIdleSway(delta);
                    this.HandleSwayLook(delta);
                    this.HandleRecoilKickBack(delta);
                    this.HandleCameraRecoil(camera, delta);

                    this.SwayWalk(delta);
                }
            }
            else
            {
                this.HandleIdleSway(delta);
                this.HandleRecoilKickBack(delta);

                if (this._ShooTest)
                {
                    this.OnShot();
                    this._ShooTest = false;
                }
            }
        }

        private Vector3 recoilKickBackPositionTemp = Vector3.Zero;
        private Vector3 recoilKickBackRotationTemp = Vector3.Zero;

        private void HandleRecoilKickBack(float delta)
        {
            // fire recoil
            if (RecoilKickBackEnabled)
            {
                rotationRecoil = rotationRecoil.Lerp(Vector3.Zero, RecoilKickBackRotationSpeed * delta);
                positionRecoil = positionRecoil.Lerp(Vector3.Zero, RecoilKickBackPositionSpeed * delta);

                recoilKickBackPositionTemp = recoilKickBackPositionTemp.Lerp(positionRecoil, RecoilKickBackPositionSpeed * delta);
                recoilKickBackRotationTemp = recoilKickBackRotationTemp.Lerp(rotationRecoil, RecoilKickBackRotationSpeed * delta);

                this.swayNodePos.Position += recoilKickBackPositionTemp;
                this.swayNodePos.Rotation += recoilKickBackRotationTemp;
            }
        }

        private void HandleCameraRecoil(CharacterCamera camera, float delta)
        {
            // fire recoil camera
            cameraRecoil = cameraRecoil.Lerp(Vector3.Zero, CameraRecoilRotationReturnSpeed * delta);
            RotCamera = RotCamera.Lerp(cameraRecoil, CameraRecoilRotationSpeed * delta);

            if (camera != null)
            {
                camera.Rotation += RotCamera;
            }
        }

        private Vector3 swayWalkTempPosition = Vector3.Zero;
        private void SwayWalk(float delta)
        {
            if (input == null)
                return;

            if (SwayWalkBobEnabled)
            {
                var currentInput = input.LastInput;

                var horizontal = currentInput.GetInput("Right") ? 1f : currentInput.GetInput("Left") ? -1f : 0f;
                var vertical = currentInput.GetInput("Back") ? 1f : currentInput.GetInput("Forward") ? -1f : 0f;
                var factor = this.BaseComponent.MovementProcessor.GetMovementSpeedFactor();

                if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
                {
                    timer = 0.0f;
                }
                else
                {
                    waveSlice = Mathf.Sin(timer);
                    timer = timer + ((SwayWalkBobSpeed / 1000) * factor);
                    if (timer > Mathf.Pi * 2)
                    {
                        timer = timer - (Mathf.Pi * 2);
                    }
                }

                var swayPos = swayWalkTempPosition;
                if (waveSlice != 0)
                {
                    float translateChange = waveSlice * ((SwayWalkBobDistance / 1000) * (factor));
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

                swayWalkTempPosition = swayPos;
                this.swayNodePos.Position += swayPos;
            }
        }

        private void HandleSwayLook(float delta)
        {
            if (this.SwayLookMoveEnabled)
            {
                // swaying on look
                targetWeaponRotation.y += -mouseMove.x * SwayLookMoveAmount * delta;
                targetWeaponRotation.x += -mouseMove.y * SwayLookMoveAmount * delta;

                targetWeaponRotation.x = Math.Clamp(targetWeaponRotation.x, -SwayLookMoveMax.x, SwayLookMoveMax.x);
                targetWeaponRotation.y = Math.Clamp(targetWeaponRotation.y, -SwayLookMoveMax.y, SwayLookMoveMax.y);

                targetWeaponRotation = VectorExtension.SmoothDamp(targetWeaponRotation, Vector3.Zero, ref targetWeaponRotationVelocity, SwayLookMoveResetSmoothing, delta);
                newWeaponRotation = VectorExtension.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, SwayLookMoveSmoothing, delta);

                var weaponRot = new Vector3(Mathf.Deg2Rad(newWeaponRotation.x), Mathf.Deg2Rad(newWeaponRotation.y), Mathf.Deg2Rad(newWeaponRotation.z));
                swayNodePos.Rotation += weaponRot;
            }
        }

        private void HandleIdleSway(float delta)
        {
            if (IdleSwayEnabled)
            {
                // idle sway
                var targetPosition = LissajousCurve(swayTime, IdleSwayAmountA, IdleSwayAmountB) / IdleSwayScale;
                newWeaponPosition = newWeaponPosition.Lerp(targetPosition, delta * IdleSwayLerpSpeed);
                swayTime += delta;
                if (swayTime > 6.3f)
                {
                    swayTime = 0;
                }

                swayNodePos.Position += newWeaponPosition;
            }
        }

        private Vector3 LissajousCurve(float Time, float A, float B)
        {
            return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.Pi), 0);
        }

        public void Tick(float delta)
        {
            if (input == null)
                return;
        }

        private void OnShot()
        {
            var rd = new RandomNumberGenerator();
            rd.Randomize();
            var rotY = rd.RandfRange(0, RecoilKickBackRotation.y);
            rd.Randomize();
            var rotZ = rd.RandfRange(0, RecoilKickBackRotation.z);
            rd.Randomize();
            var kickBackX = rd.RandfRange(0, RecoilKickBackPosition.x);
            rd.Randomize();
            var kickBackY = rd.RandfRange(0, RecoilKickBackPosition.y);

            rotationRecoil += new Vector3(Mathf.Deg2Rad(RecoilKickBackRotation.x), Mathf.Deg2Rad(rotY), Mathf.Deg2Rad(rotZ));
            positionRecoil += new Vector3(kickBackX, kickBackY, RecoilKickBackPosition.z);
            rd.Randomize();

            var cameraRecoilY = rd.RandfRange(-CameraRecoilPosition.y, CameraRecoilPosition.y);
            rd.Randomize();
            var cameraRecoilZ = rd.RandfRange(-CameraRecoilPosition.z, CameraRecoilPosition.z);

            cameraRecoil += new Vector3(Mathf.Deg2Rad(CameraRecoilPosition.x), Mathf.Deg2Rad(cameraRecoilY), Mathf.Deg2Rad(cameraRecoilZ));
        }
    }
}
