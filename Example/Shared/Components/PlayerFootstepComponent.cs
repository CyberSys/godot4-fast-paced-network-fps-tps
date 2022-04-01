using System.Net;
using System.IO;
using Framework;
using Framework.Game.Client;
using Framework.Physics;
using Framework.Network;
using Godot;
using System;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Framework.Utils;
namespace Shooter.Shared.Components
{
    public enum GroundType
    {
        None = 0,
        Metal = 1,
    }
    public struct FootStepsPackage : INetSerializable, IEquatable<FootStepsPackage>
    {
        public int SoundIndex { get; set; }
        public GroundType GroundType { get; set; }
        public float CurrentTime { get; set; }
        public float PitchScale { get; set; }
        public float UnitDB { get; set; }
        public float ZPos { get; set; }

        public bool Equals(FootStepsPackage compareObj)
        {
            return this.GetHashCode() == compareObj.GetHashCode();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SoundIndex);
            writer.Put((int)GroundType);
            writer.Put(CurrentTime);
            writer.Put(PitchScale);
            writer.Put(UnitDB);
            writer.Put(ZPos);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            SoundIndex = reader.GetInt();
            GroundType = (GroundType)reader.GetInt();
            CurrentTime = reader.GetFloat();
            PitchScale = reader.GetFloat();
            UnitDB = reader.GetFloat();
            ZPos = reader.GetFloat();
        }
    }

    public partial class PlayerFootstepComponent : AudioStreamPlayer3D, IChildNetworkSyncComponent<FootStepsPackage>, IPlayerComponent
    {
        [Export(PropertyHint.Dir)]
        public string FootStepsSoundPathFolder = "res://Assets/Audio/Footsteps/";

        public IBaseComponent BaseComponent { get; set; }
        private AsyncLoader soundLoader = new AsyncLoader();
        private int currentStep = 0;
        private float nextStepSound = 0.0f;
        public const float betweenStepMultiplier = 3.70f / 2f;

        private FootStepsPackage CurrentFootstep { get; set; } = new FootStepsPackage { };
        private GroundType CurrentGround { get; set; } = GroundType.None;

        public override void _EnterTree()
        {
            base._EnterTree();
            this.Bus = "PlayerFoorSteps";
        }

        public string GetComponentName()
        {
            return "footsteps";
        }

        public void Tick(float delta)
        {
            var body = this.BaseComponent.Components.Get<PlayerBodyComponent>();
            if (body != null && !(this.BaseComponent is PuppetPlayer))
            {
                if (body.IsOnGround())
                {
                    var collision = body.GetLastSlideCollision();
                    if (collision == null)
                        return;

                    var collider = collision.GetCollider();

                    if (collider != null && collider is StaticBody3D)
                    {
                        var positon = collision.GetPosition(0);

                        var transform = body.Transform;
                        transform.origin = positon;
                        this.Transform = transform;

                        var footstepSet = (collider as StaticBody3D).GetMeta("footstep");
                        try
                        {
                            this.CurrentGround = (GroundType)Enum.Parse(typeof(GroundType), footstepSet.ToString());
                        }
                        catch
                        {
                            this.CurrentGround = GroundType.None;
                        }
                        this.CheckFootstep(body);
                    }
                }
            }
        }


        private FootStepsPackage LastFootStep;
        public void ApplyNetworkState(FootStepsPackage package)
        {
            if (this.BaseComponent is PuppetPlayer)
            {
                //stop directly for syncronisation
                if (package.Equals(default(FootStepsPackage)))
                {
                    this.Stop();
                }
                else
                {
                    if (!LastFootStep.Equals(default(FootStepsPackage)))
                    {
                        if (!LastFootStep.Equals(package))
                        {
                            this.Play(package);
                        }
                    }
                    else
                    {
                        this.Play(package);
                    }
                }

                this.LastFootStep = package;
            }
        }

        private void Play(FootStepsPackage package)
        {
            if (!package.Equals(default(FootStepsPackage)))
            {
                this.CacheFiles(package.GroundType);
                if (this.soundsCache.ContainsKey(package.GroundType))
                {
                    var groundFiles = this.soundsCache[package.GroundType];
                    if (groundFiles.Count - 1 >= package.SoundIndex)
                    {
                        var audioFile = groundFiles[package.SoundIndex].AudioFile;
                        if (audioFile != null)
                        {
                            this.PitchScale = package.PitchScale;
                            this.UnitDb = package.UnitDB;
                            this.Stream = audioFile;

                            var pos = this.Position;
                            pos.z = package.ZPos;
                            this.Position = pos;

                            this.Play(package.CurrentTime);
                        }
                    }
                }
            }
        }

        public FootStepsPackage GetNetworkState()
        {
            if (!this.Playing)
            {
                return new FootStepsPackage();
            }
            else
            {
                var step = CurrentFootstep;
                step.CurrentTime = this.GetPlaybackPosition();
                return step;
            }
        }

        [Export]
        public Vector3 audioOffset = Vector3.Zero;


        private void CheckFootstep(PlayerBodyComponent body)
        {
            var speed = (body.MovementProcessor as DefaultMovementProcessor).GetMovementSpeedFactor();
            var minSpeed = (body.MovementProcessor as DefaultMovementProcessor).GetWalkingSpeed();
            var velocity = (body.MovementProcessor as DefaultMovementProcessor).Velocity;

            if (body.IsOnGround())
            {
                if (velocity.Length() >= minSpeed && this.nextStepSound <= 0.0f)
                {
                    this.playFootstep();
                    this.nextStepSound = 1.0f;
                }
                else
                {
                    var nextStepReduce = (float)this.GetPhysicsProcessDeltaTime() * speed * betweenStepMultiplier;
                    this.nextStepSound -= nextStepReduce;
                }

                if (velocity.Length() < minSpeed)
                {
                    this.Stop();
                }
            }
            else
            {
                this.nextStepSound = 0.0f;
            }
        }

        public struct FootStepsSound
        {
            public string Filename { get; set; }
            public AudioStreamSample AudioFile { get; set; }
        }

        public Dictionary<GroundType, List<FootStepsSound>> soundsCache = new Dictionary<GroundType, List<FootStepsSound>>();

        public void CacheFiles(GroundType folderName)
        {
            if (!soundsCache.ContainsKey(folderName))
            {
                soundsCache.Add(folderName, new List<FootStepsSound>());
                var footstepPath = System.IO.Path.Combine(this.FootStepsSoundPathFolder, folderName.ToString());

                var dir = new Godot.Directory();
                var listOfFiles = new List<string>();
                if (dir.Open(footstepPath) == Error.Ok)
                {
                    dir.ListDirBegin();
                    var file_name = dir.GetNext();
                    while (!String.IsNullOrEmpty(file_name))
                    {
                        if (!dir.CurrentIsDir() && (file_name.EndsWith(".wav")))
                        {
                            var detectedFile = System.IO.Path.Combine(footstepPath, file_name);
                            listOfFiles.Add(detectedFile);
                            this.soundLoader.LoadResource(detectedFile, (res) =>
                            {
                                soundsCache[folderName].Add(new FootStepsSound
                                {
                                    Filename = detectedFile,
                                    AudioFile = res as AudioStreamSample
                                });
                            });
                        }

                        file_name = dir.GetNext();
                    }
                }
            }
        }


        public override void _Process(float delta)
        {
            base._Process(delta);
            this.soundLoader.Tick();
        }

        private void playFootstep()
        {
            if (CurrentGround != GroundType.None)
                return;

            System.Random rnd = new System.Random();
            this.CacheFiles(CurrentGround);

            if (this.soundsCache.ContainsKey(CurrentGround))
            {
                var files = this.soundsCache[CurrentGround];
                if (files.Count <= 0)
                {
                    return;
                }

                int index = rnd.Next(0, files.Count - 1);
                var item = files[index];

                var randm = new RandomNumberGenerator();
                randm.Randomize();
                var scale = randm.RandfRange(0.8f, 1.2f);
                randm.Randomize();
                var unitdb = randm.RandfRange(0.8f, 1.0f);
                this.CurrentFootstep = new FootStepsPackage
                {
                    SoundIndex = index,
                    GroundType = CurrentGround,
                    ZPos = (currentStep % 2 == 0) ? -0.3f : 0.3f,
                    UnitDB = unitdb,
                    PitchScale = scale,
                    CurrentTime = 0,
                };

                currentStep++;

                if (currentStep == 100)
                    currentStep = 0;

                Play(CurrentFootstep);
            }
        }
    }
}
