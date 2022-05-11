/*
 * Created on Mon Mar 28 2022
 *
 * The MIT License (MIT)
 * Copyright (c) 2022 Stefan Boronczyk, Striked GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Framework.Network;
using Framework.Game;
using System.Collections.Generic;
using Framework.Network.Commands;
using System.Linq;
using LiteNetLib.Utils;
using Framework.Input;
using Framework.Physics;
using Godot;
using Framework.Game.Server;
using System;
using Framework.Game.Client;
using System.Reflection;

namespace Framework.Game
{
    /// <summary>
    /// The general player class
    /// </summary>
    public partial class NetworkCharacter : Godot.CharacterBody3D, INetworkCharacter, IBaseComponent
    {
        /// <summary>
        /// The mono script path of the player component
        /// </summary>
        /// <value></value>
        public string[] ScriptPaths { get; set; } = new string[0];

        /// <summary>
        /// The resource path of the player component
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; } = "";

        /// <summary>
        /// Movement processor
        /// </summary>
        /// <returns></returns>
        public IMovementProcessor MovementProcessor { get; set; } = new DefaultMovementProcessor();

        /// <summary>
        /// The network mode of the plaer
        /// </summary>
        /// <value></value>
        [Export(PropertyHint.Enum)]
        public NetworkMode Mode { get; set; }

        /// <summary>
        /// Node path to collider
        /// </summary>
        [Export]
        public NodePath ColliderPath = null;

        /// <summary>
        /// The height on crouching
        /// </summary>
        [Export]
        public float CrouchHeight = 1.3f;


        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet)]
        public Vector3 NetworkVelocity = Vector3.Zero;

        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet)]
        public float NetworkCrouchingLevel = 1.0f;

        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet)]
        public Quaternion NetworkRotation = Quaternion.Identity;

        [NetworkVar(NetworkSyncFrom.FromServer, NetworkSyncTo.ToClient | NetworkSyncTo.ToPuppet)]
        public Vector3 NetworkPosition = Vector3.Zero;

        internal float originalHeight = 0f;
        internal CollisionShape3D shape;
        internal float originalYPosition = 0f;
        internal float shapeHeight = 0.0f;
        internal float previousCrouchLevel = 0f;

        internal MeshInstance3D debugMesh;
        internal MeshInstance3D debugMeshLocal;

        internal Queue<PlayerState> stateQueue = new Queue<PlayerState>();
        internal PlayerState? lastState = null;
        internal float stateTimer = 0;

        private Queue<Vector3> teleportQueue = new Queue<Vector3>();

        /// <summary>
        /// Get the current shape height
        /// </summary>
        /// <returns></returns>
        public float GetShapeHeight()
        {
            if (this.shape != null)
                return this.shape.Transform.origin.y;
            else
                return 0;
        }

        /// <summary>
        /// Player sync attributes
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="NetworkAttribute"></typeparam>
        /// <returns></returns>
        public Dictionary<string, NetworkAttribute> NetworkSyncVars { get; private set; } = new Dictionary<string, NetworkAttribute>();

        private void CreateDebugMesh()
        {
            var shape = this.GetShape();
            if (shape != null && shape.Shape != null && shape.Shape is CapsuleShape3D)
            {
                var radShape = shape.Shape as CapsuleShape3D;
                var cpsule = new CapsuleMesh();
                cpsule.Radius = radShape.Radius;
                cpsule.Height = radShape.Height;

                var mat = new StandardMaterial3D();
                mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                var color = Colors.Red;
                color.a = 0.3f;
                mat.AlbedoColor = color;

                var debugMeshLocalMat = new StandardMaterial3D();
                debugMeshLocalMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                var colorLocal = Colors.Green;
                colorLocal.a = 0.3f;
                debugMeshLocalMat.AlbedoColor = colorLocal;

                var mesh = new MeshInstance3D();
                mesh.Mesh = cpsule;
                mesh.MaterialOverride = mat;
                mesh.Name = "DebugMesh";
                mesh.Visible = false;

                var meshLocal = new MeshInstance3D();
                meshLocal.Mesh = cpsule;
                meshLocal.MaterialOverride = debugMeshLocalMat;
                meshLocal.Name = "DebugMeshLocal";
                meshLocal.Visible = false;

                this.AddChild(mesh);
                this.AddChild(meshLocal);

                this.debugMesh = mesh;
                this.debugMeshLocal = meshLocal;
            }
        }

        /// <summary>
        /// Get the shape of the player body
        /// </summary>
        /// <returns></returns>
        public CollisionShape3D GetShape()
        {
            return this.GetNodeOrNull<CollisionShape3D>(ColliderPath); ;
        }

        internal void SetNewComponent(Node child)
        {
            if (child is IPlayerComponent)
            {
                var node = child as IPlayerComponent;
                this.ActivateComponent(node.NetworkId, false);

                Logger.LogDebug(this, "Found component: " + node.GetType().Name);
                (child as IPlayerComponent).BaseComponent = this;
            }
        }

        /// <inheritdoc />
        public override void _EnterTree()
        {
            base._EnterTree();

            this.ChildEnteredTree += (Node n) =>
            {
                SetNewComponent(n);
            };

            this.shape = this.GetShape();
            this.shape.Shape.ResourceLocalToScene = true;

            float shapeHeight = 0;

            if (shape != null)
            {
                this.originalHeight = this.shape.Scale.y;
                this.originalYPosition = this.shape.Transform.origin.y;
                if (this.shape.Shape is CapsuleShape3D)
                {
                    shapeHeight = (this.shape.Shape as CapsuleShape3D).Height;
                }

                else if (this.shape.Shape is BoxShape3D)
                {
                    shapeHeight = (this.shape.Shape as BoxShape3D).Size.y;
                }
                else
                {
                    throw new Exception("Shape type not implemented yet");
                }
            }

            this.shapeHeight = shapeHeight;
            //   this.NetworkCrouchingLevel = shapeHeight;
            this.previousCrouchLevel = shapeHeight;

            if (this.IsLocal())
            {
                this.CreateDebugMesh();
            }
        }

        /// <summary>
        /// Activate an component by given id
        /// </summary>
        /// <param name="index"></param>
        /// <param name="enable"></param>
        public void ActivateComponent(int index, bool enable)
        {
            var node = this.Components.All.Where(df => df is IPlayerComponent && df is Node).Select(df => df as IPlayerComponent)
            .FirstOrDefault(df => df.NetworkId == index);

            if (node == null || !node.IsInsideTree())
                return;

            var child = node as Node;
            child.ProcessMode = enable ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

            if (child is IPlayerComponent)
            {
                (child as IPlayerComponent).IsEnabled = enable;
            }

            if (child is Node3D)
            {
                (child as Node3D).Visible = enable;
            }

            child.SetPhysicsProcess(enable);
            child.SetProcessInternal(enable);
            child.SetProcess(enable);
            child.SetProcessInput(enable);
            child.SetPhysicsProcessInternal(enable);
        }

        /// <summary>
        /// Is player on ground
        /// </summary>
        /// <returns>true or false</returns>
        public virtual bool IsOnGround()
        {
            return this.IsOnFloor();
        }

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            if (Godot.Input.IsActionJustPressed("test") && this.IsLocal() && !this.GameWorld.GameInstance.GuiDisableInput)
            {
                var vel = this.GlobalTransform;
                vel.origin.x += 5;
                this.GlobalTransform = vel;
            }

            base._Process(delta);

            var camera = this.Components.Get<CharacterCamera>();
            if (this.IsLocal() && this.debugMesh != null && this.debugMeshLocal != null && camera != null)
            {
                var activated = ClientSettings.Variables.Get<bool>("cl_debug_server", false);
                if (activated == true && camera.Mode == CameraMode.FPS)
                    activated = false;

                if (!activated)
                {
                    this.debugMeshLocal.Visible = false;
                    this.debugMesh.Visible = false;
                }
                else
                {
                    var state = this.IncomingLocalPlayerState;
                    if (!default(PlayerState).Equals(state))
                    {
                        this.debugMeshLocal.Visible = activated;
                        this.debugMesh.Visible = activated;

                        this.debugMesh.GlobalTransform = new Transform3D(
                            state.GetVar<Quaternion>(this, "NetworkRotation"),
                            state.GetVar<Vector3>(this, "NetworkPosition"));
                    }
                }
            }
        }

        /// <summary>
        /// Set the shape height by a given process value 
        /// </summary>
        /// <param name="crouchInProcess">can be between 0 (crouching) and 1 (non crouching)</param>
        public virtual void SetCrouchingLevel(float crouchInProcess)
        {
            // this.currentCouchLevel
            this.NetworkCrouchingLevel += MathF.Round(crouchInProcess, 2);
            this.NetworkCrouchingLevel = Mathf.Clamp(this.NetworkCrouchingLevel, CrouchHeight, this.shapeHeight);
        }

        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void MoveToPosition(Godot.Vector3 origin)
        {
            var data = this.ToNetworkState();
            data.SetVar(this, "NetworkPosition", origin);
            this.ApplyBodyState(data);
        }

        /// <inheritdoc />
        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (this.previousCrouchLevel != this.NetworkCrouchingLevel)
            {
                if (this.shape != null && this.shape.Shape != null)
                {
                    var yPos = (this.shapeHeight - this.NetworkCrouchingLevel) / 2;
                    yPos = yPos * -1;

                    var transform = this.shape.Transform;
                    transform.origin.y = this.originalYPosition + yPos;
                    transform.origin.y = Mathf.Clamp(transform.origin.y, (this.shapeHeight / 2) * -1f, this.originalYPosition);
                    this.shape.Transform = transform;

                    var shape = this.shape.Shape as CapsuleShape3D;
                    shape.Height = NetworkCrouchingLevel;
                    this.shape.Shape = shape;

                    this.previousCrouchLevel = this.NetworkCrouchingLevel;
                }
            }

            if (this.IsPuppet())
            {
                this.ProcessPuppetInput(delta);
            }
        }


        /// <summary>
        /// Trigger an attack on server side
        /// </summary>
        /// <param name="range">Range for detection</param>
        public void DoAttack(float range = 1000)
        {
            if (this.GameWorld is NetworkServerWorld)
                (this.GameWorld as NetworkServerWorld).ProcessPlayerAttack(this, range);
        }


        /// <summary>
        /// Trigger when an player got an hit
        /// </summary>
        public virtual void OnHit(RayCastHit hit)
        {

        }

        /// <summary>
        /// Time since last disconnect
        /// </summary>
        /// <value></value>
        public float DisconnectTime { get; set; } = 0;

        /// <summary>
        /// Get the last tick of the last input
        /// </summary>
        /// <value></value>
        public uint LatestInputTick { get; set; } = 0;

        /// <summary>
        /// Return if the player is syncronized with server.
        /// </summary>
        /// <value></value>
        public bool IsSynchronized { get; set; } = false;


        /// <summary>
        /// Archived player states
        /// </summary>
        public PlayerState[] States = new PlayerState[NetworkWorld.MaxTicks];

        /// <summary>
        /// The active tick based input
        /// </summary>
        /// <value></value>
        // Current input struct for each player.
        // This is only needed because the ProcessAttack delegate flow is a bit too complicated.
        // TODO: Simplify this.
        public TickInput CurrentPlayerInput { get; set; }

        /// <inheritdoc />
        public short[] RequiredPuppetComponents { get; set; } = new short[0];

        /// <inheritdoc />
        public short[] RequiredComponents { get; set; } = new short[0];

        /// <inheritdoc />
        public short NetworkId { get; set; }

        /// <inheritdoc />
        public string PlayerName { get; set; } = "";

        /// <inheritdoc />
        public int Latency { get; set; }

        /// <inheritdoc />
        public PlayerConnectionState State { get; set; }

        /// <inheritdoc />
        public INetworkWorld GameWorld { get; set; }

        /// <inheritdoc />
        private readonly ComponentRegistry<NetworkCharacter> _components;

        /// <inheritdoc />
        public ComponentRegistry<NetworkCharacter> Components => _components;

        /// <summary>
        /// Previous state since last tick
        /// </summary>
        /// <value></value>
        public PlayerConnectionState PreviousState { get; set; }


        /// <summary>
        /// The last incoming local player state
        /// </summary>
        /// <returns></returns>
        public PlayerState IncomingLocalPlayerState = new PlayerState();

        /// <summary>
        /// Base player class
        /// </summary>
        /// <returns></returns>
        public NetworkCharacter() : base()
        {
            this.NetworkSyncVars = this.GetNetworkAttributes();
            this._components = new ComponentRegistry<NetworkCharacter>(this);
        }

        /// <inheritdoc />
        public virtual void Tick(float delta)
        {
        }

        /// <summary>
        /// /// Get the current network state
        /// </summary>
        /// <returns></returns>
        public virtual PlayerState ToNetworkState()
        {
            var state = new PlayerState
            {
                NetworkId = this.NetworkId,
                Latency = (short)this.Latency
            };

            state.NetworkSyncedVars = new List<PlayerNetworkVarState>();
            foreach (var element in NetworkSyncVars)
            {
                state.SetVar(this, element.Key, this.Get(element.Key));
            }

            state.SetVar(this, "NetworkRotation", this.GlobalTransform.basis.GetRotationQuaternion());
            state.SetVar(this, "NetworkPosition", this.GlobalTransform.origin);
            state.SetVar(this, "NetworkVelocity", this.MovementProcessor.Velocity);
            state.SetVar(this, "NetworkCrouchingLevel", this.NetworkCrouchingLevel);

            return state;
        }

        /// <summary>
        /// Detect an hit by given camera view
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public virtual RayCastHit DetechtHit(float range)
        {
            var input = this.Components.Get<NetworkInput>();
            if (input == null)
                return null;

            var camera = this.Components.Get<CharacterCamera>();
            if (camera == null)
                return null;

            var headViewRotation = input.LastInput.ViewDirection;
            var basis = new Basis(headViewRotation);

            var command = this.ToNetworkState();
            var currentTransform = new Godot.Transform3D(basis, command.GetVar<Vector3>(this, "NetworkPosition"));

            var attackPosition = camera.GlobalTransform.origin;
            var attackTransform = new Godot.Transform3D(basis, attackPosition);
            var attackTransformFrom = new Godot.Transform3D(command.GetVar<Quaternion>(this, "NetworkRotation"), attackPosition);

            var raycast = new PhysicsRayQueryParameters3D();
            raycast.From = attackTransformFrom.origin;
            raycast.To = attackTransform.origin + -attackTransform.basis.z * range;

            var result = GetWorld3d().DirectSpaceState.IntersectRay(raycast);

            if (result != null && result.Contains("position"))
            {
                var rayResult = new RayCastHit
                {
                    PlayerSource = this,
                    To = (Vector3)result["position"],
                    Collider = (Node)result["collider"],
                    From = attackTransform.origin
                };

                if (rayResult.Collider is HitBox)
                {
                    var enemy = (rayResult.Collider as HitBox).GetPlayer();
                    if (enemy != null && enemy is INetworkCharacter)
                    {
                        rayResult.PlayerDestination = enemy;
                    }
                }

                return rayResult;
            }

            else return null;
        }

        /// <summary>
        /// Apply the network state for the body component
        /// </summary>
        /// <param name="state"></param>
        public void ApplyBodyState(PlayerState state)
        {
            this.GlobalTransform = new Transform3D(
                state.GetVar<Quaternion>(this, "NetworkRotation"),
            state.GetVar<Vector3>(this, "NetworkPosition"));

            this.MovingPlatformApplyVelocityOnLeave = MovingPlatformApplyVelocityOnLeaveEnum.Never;

            this.Velocity = state.GetVar<Vector3>(this, "NetworkVelocity");
            this.MovementProcessor.Velocity = state.GetVar<Vector3>(this, "NetworkVelocity");
        }


        /// <summary>
        /// Moving the character
        /// </summary>
        /// <param name="delta">float</param>
        /// <param name="velocity">Vector3</param>
        public virtual void Move(float delta, Vector3 velocity)
        {
            this.Velocity = velocity;
            this.MoveAndSlide();
        }

        public void ApplyVars(PlayerState state)
        {
            if (this.IsPuppet())
            {
                // check if the marked for local
                foreach (var networkVar in this.NetworkSyncVars.Where(df => df.Value.From.HasFlag(NetworkSyncFrom.FromServer)
                && df.Value.To.HasFlag(NetworkSyncTo.ToPuppet)))
                {
                    var value = state.GetVar(this, networkVar.Key);
                    this.Set(networkVar.Key, value);
                }
            }

            if (this.IsLocal())
            {
                // check if the marked for local
                foreach (var networkVar in this.NetworkSyncVars.Where(df => df.Value.From.HasFlag(NetworkSyncFrom.FromServer)
                && df.Value.To.HasFlag(NetworkSyncTo.ToClient)))
                {
                    var value = state.GetVar(this, networkVar.Key);
                    this.Set(networkVar.Key, value);
                }
            }
        }

        public virtual PlayerState Interpolate(float theta, PlayerState lastState, PlayerState nextState)
        {
            //only for movement component (to interpolate)
            var a = lastState.GetVar<Quaternion>(this, "NetworkRotation");
            var b = nextState.GetVar<Quaternion>(this, "NetworkRotation");

            var aPos = lastState.GetVar<Vector3>(this, "NetworkPosition");
            var bPos = nextState.GetVar<Vector3>(this, "NetworkPosition");

            var aCrouch = lastState.GetVar<float>(this, "NetworkCrouchingLevel");
            var bCrouch = nextState.GetVar<float>(this, "NetworkCrouchingLevel");

            var newState = this.ToNetworkState();

            newState.SetVar(this, "NetworkPosition", aPos.Lerp(bPos, theta));
            newState.SetVar(this, "NetworkRotation", a.Slerp(b, theta));
            newState.SetVar(this, "NetworkCrouchingLevel", Mathf.Lerp(aCrouch, bCrouch, theta));

            return newState;
        }

        /// <summary>
        /// Apply an network state
        /// </summary>
        /// <param name="state">The network state to applied</param>
        public virtual void ApplyNetworkState(PlayerState state)
        {
            if (!this.IsPuppet() || !this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                this.ApplyVars(state);
                this.ApplyBodyState(state);
            }
            // only for puppet
            else
            {
                while (stateQueue.Count >= 2)
                {
                    stateQueue.Dequeue();
                }
                stateQueue.Enqueue(state);
            }
        }

        internal void ProcessPuppetInput(float delta)
        {
            if (!this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                return;
            }

            stateTimer += delta;

            float SimulationTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            float ServerSendRate = SimulationTickRate / 2;
            float ServerSendInterval = 1f / ServerSendRate;

            if (stateTimer > ServerSendInterval)
            {
                stateTimer -= ServerSendInterval;
                if (stateQueue.Count > 1)
                {
                    lastState = stateQueue.Dequeue();
                }
            }

            // We can only interpolate if we have a previous and next world state.
            if (!lastState.HasValue || stateQueue.Count < 1)
            {
                //Logger.LogDebug(this, "RemotePlayer: not enough states to interp");
                return;
            }

            var nextState = stateQueue.Peek();
            float theta = stateTimer / ServerSendInterval;

            if (this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                var newState = this.Interpolate(theta, lastState.Value, nextState);
                this.ApplyVars(newState);
                this.ApplyBodyState(newState);
            }
            else
            {
                this.ApplyVars(nextState);
                this.ApplyBodyState(nextState);
            }
        }

        internal virtual void InternalTick(float delta)
        {
            if (this.teleportQueue.Count > 0)
            {
                var nextTeleport = this.teleportQueue.Dequeue();
                Logger.LogDebug(this, "Execute teleport to " + nextTeleport + " -> " + this.IsServer());
                this.MoveToPosition(nextTeleport);

                Logger.LogDebug(this, "Finish teleport to " + nextTeleport + " -> " + this.IsServer());
            }

            //process server and client
            if (!this.IsPuppet())
            {
                var input = this.Components.Get<NetworkInput>();
                if (input != null)
                {
                    // this.MovementProcessor.Velocity = this.Velocity;
                    this.MovementProcessor.SetServerVars(this.GameWorld.ServerVars);
                    this.MovementProcessor.SetClientVars(Framework.Game.Client.ClientSettings.Variables);
                    this.MovementProcessor.Simulate(this, input.LastInput, delta);
                }
            }

            //process components
            foreach (var component in this.Components.All)
            {
                if (component is IPlayerComponent)
                {
                    (component as IPlayerComponent)?.Tick(delta);
                }
            }

            this.Tick(delta);
        }

        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void DoTeleport(Godot.Vector3 origin)
        {
            Logger.LogDebug(this, "Enquene teleport to " + origin);
            this.teleportQueue.Enqueue(origin);
        }
    }
}