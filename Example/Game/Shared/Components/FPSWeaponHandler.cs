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
    public struct PlayerWeaponPackage : INetSerializable
    {
        public short WeaponIndex { get; set; }

        public bool IsFired { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(WeaponIndex);
            writer.Put(IsFired);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            WeaponIndex = reader.GetShort();
            IsFired = reader.GetBool();
        }
    }

    public partial class FPSWeaponHandler : Node, IPlayerComponent
    {
        public short NetworkId { get; set; } = 9;

        [Export]
        public bool IsEnabled { get; set; } = false;

        public Framework.Game.NetworkCharacter BaseComponent { get; set; }

        [Export]
        public AudioStreamSample ShootSound { get; set; }

        [Export]
        public NodePath ShootSoundPlayerPath { get; set; }

        [Export]
        public AudioStreamPlayer3D ShootSoundPlayer { get; set; }

        [Export]
        public NodePath inputPath { get; set; }

        [Export]
        public float fireRate = 0.1f;

        [Export]
        public bool requiredFire = true;

        private float fireRateTimer = 0f;
        private bool previousPressedState = false;

        private CharacterCamera camera;

        private NetworkInput input;

        public override void _EnterTree()
        {
            base._EnterTree();

            input = this.GetNodeOrNull<NetworkInput>(inputPath);
            ShootSoundPlayer = this.GetNodeOrNull<AudioStreamPlayer3D>(ShootSoundPlayerPath);
        }

        public void CheckShoot(bool isPressed, float delta)
        {
            if (!requiredFire)
            {
                if (fireRateTimer <= 0f && isPressed)
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
                if (previousPressedState == false && isPressed && fireRateTimer <= 0f)
                {
                    this.AddShot();
                    this.fireRateTimer = this.fireRate;
                }
                else
                {
                    fireRateTimer -= delta;
                }
                previousPressedState = isPressed;
            }
        }

        public void Tick(float delta)
        {
            if (input == null)
                return;

            if (this.IsLocal() || this.IsServer())
            {
                this.CheckShoot(input.LastInput.GetInput("Fire"), delta);
            }
        }

        private bool shootTemporary = false;

        private void AddShot()
        {
            if (!Engine.IsEditorHint() && this.BaseComponent.IsServer())
                this.BaseComponent.DoAttack();

            if (this.BaseComponent.IsLocal())
            {
                if (this.ShootSoundPlayer != null && this.ShootSound != null)
                {
                    this.ShootSoundPlayer.Stream = this.ShootSound;
                    this.ShootSoundPlayer.Play();
                }
            }
        }

        public void ApplyNetworkState(PlayerWeaponPackage package)
        {
            if (this.IsPuppet() && package.IsFired)
            {
                GD.Print("PUPPET FIRE:");
            }
        }

        public PlayerWeaponPackage GetNetworkState()
        {
            if (this.IsServer())
            {
                return new PlayerWeaponPackage
                {
                    IsFired = this.shootTemporary
                };
            }
            return new PlayerWeaponPackage
            {

            };
        }
    }
}
