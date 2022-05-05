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
using Framework.Physics.Commands;
using Framework.Game.Client;


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
        public string ScriptPath { get; set; }

        /// <summary>
        /// The resource path of the player component
        /// </summary>
        /// <value></value>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Movement processor
        /// </summary>
        /// <returns></returns>
        public IMovementProcessor MovementProcessor { get; set; } = new DefaultMovementProcessor();

        /// <summary>
        /// The network mode of the plaer
        /// </summary>
        /// <value></value>
        [Export]
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

        internal float originalHeight = 0f;
        internal CollisionShape3D shape;
        internal float originalYPosition = 0f;
        internal float shapeHeight = 0.0f;
        internal float previousCrouchLevel = 0f;
        internal float currentCouchLevel = 0f;

        internal MeshInstance3D debugMesh;
        internal MeshInstance3D debugMeshLocal;

        internal Queue<PlayerState> stateQueue = new Queue<PlayerState>();
        internal PlayerState? lastState = null;
        internal float stateTimer = 0;

        /// <inheritdoc />
        internal int replayedStates;

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

        /// <summary>
        /// Get the current physics state for the body component
        /// </summary>
        /// <returns></returns>
        public MovementNetworkCommand GetNetworkState()
        {
            return new MovementNetworkCommand
            {
                Position = this.GlobalTransform.origin,
                Rotation = this.GlobalTransform.basis.GetRotationQuaternion(),
                Velocity = this.MovementProcessor.Velocity,
                Grounded = this.IsOnGround(),
            };
        }


        /// <inheritdoc />
        public override void _EnterTree()
        {
            base._EnterTree();

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
            this.currentCouchLevel = shapeHeight;
            this.previousCrouchLevel = shapeHeight;

            if (this.IsLocal())
            {
                this.CreateDebugMesh();
            }
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
                        var incomingStateDecompose = state.BodyComponent;
                        this.debugMeshLocal.Visible = activated;
                        this.debugMesh.Visible = activated;

                        var gt = this.debugMesh.GlobalTransform;
                        gt.origin = incomingStateDecompose.Position;
                        gt.basis = new Basis(incomingStateDecompose.Rotation);
                        this.debugMesh.GlobalTransform = gt;
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
            this.currentCouchLevel += MathF.Round(crouchInProcess, 2);
            this.currentCouchLevel = Mathf.Clamp(this.currentCouchLevel, CrouchHeight, this.shapeHeight);
        }

        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void MoveToPosition(Godot.Vector3 origin)
        {
            var data = this.GetNetworkState();
            data.Position = origin;
            this.ApplyBodyState(data);
        }

        /// <inheritdoc />
        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (this.previousCrouchLevel != this.currentCouchLevel)
            {
                if (this.shape != null && this.shape.Shape != null)
                {
                    var yPos = (this.shapeHeight - this.currentCouchLevel) / 2;
                    yPos = yPos * -1;

                    var transform = this.shape.Transform;
                    transform.origin.y = this.originalYPosition + yPos;
                    transform.origin.y = Mathf.Clamp(transform.origin.y, (this.shapeHeight / 2) * -1f, this.originalYPosition);
                    this.shape.Transform = transform;

                    var shape = this.shape.Shape as CapsuleShape3D;
                    shape.Height = currentCouchLevel;
                    this.shape.Shape = shape;

                    this.previousCrouchLevel = this.currentCouchLevel;
                }
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
        public int[] RequiredPuppetComponents { get; set; } = new int[0];

        /// <inheritdoc />
        public int[] RequiredComponents { get; set; } = new int[0];
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string PlayerName { get; set; }

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
        /// The local player input snapshots
        /// </summary>
        public GeneralPlayerInput[] localPlayerInputsSnapshots = new GeneralPlayerInput[NetworkWorld.MaxTicks];

        /// <summary>
        /// The local player states
        /// </summary>
        public PlayerState[] localPlayerStateSnapshots = new PlayerState[NetworkWorld.MaxTicks];

        /// <summary>
        /// The last world player ticks related to the state snapshots
        /// </summary>
        public uint[] localPlayerWorldTickSnapshots = new uint[NetworkWorld.MaxTicks];


        /// <inheritdoc />
        private readonly List<AssignedComponent> avaiableComponents = new List<AssignedComponent>();

        /// <inheritdoc />
        public List<AssignedComponent> AvaiablePlayerComponents => avaiableComponents;

        /// <summary>
        /// Base player class
        /// </summary>
        /// <returns></returns>
        public NetworkCharacter() : base()
        {
            this._components = new ComponentRegistry<NetworkCharacter>(this);
        }

        /// <inheritdoc />
        public virtual void Tick(float delta)
        {
        }

        /// <summary>
        /// Get the current network state
        /// </summary>
        /// <returns></returns>
        public virtual PlayerState ToNetworkState()
        {
            var netComps = new System.Collections.Generic.Dictionary<int, byte[]>();
            foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
            {
                var findIndex = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());
                if (findIndex > -1)
                {
                    var instanceMethod = component.GetType().GetMethod("GetNetworkState");
                    INetSerializable result = instanceMethod.Invoke(component, new object[] { }) as INetSerializable;

                    var writer = new NetDataWriter();
                    result.Serialize(writer);
                    netComps.Add(findIndex, writer.Data);
                }
            }

            return new PlayerState
            {
                Id = this.Id,
                BodyComponent = this.GetNetworkState(),
                NetworkComponents = netComps
            };
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

            var command = this.GetNetworkState();
            var currentTransform = new Godot.Transform3D(basis, command.Position);

            var attackPosition = camera.GlobalTransform.origin;
            var attackTransform = new Godot.Transform3D(basis, attackPosition);
            var attackTransformFrom = new Godot.Transform3D(command.Rotation, attackPosition);

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
        public void ApplyBodyState(MovementNetworkCommand state)
        {
            var transform = this.GlobalTransform;
            transform.origin = state.Position;
            transform.basis = new Basis(state.Rotation);
            this.GlobalTransform = transform;

            this.Velocity = state.Velocity;
            this.MovementProcessor.Velocity = state.Velocity;
        }


        /// <summary>
        /// Moving the character
        /// </summary>
        /// <param name="delta">float</param>
        /// <param name="velocity">Vector3</param>
        public virtual void Move(float delta, Vector3 velocity)
        {
            this.MoveAndSlide();
        }

        /// <summary>
        /// Apply an network state
        /// </summary>
        /// <param name="state">The network state to applied</param>
        public virtual void ApplyNetworkState(PlayerState state)
        {
            if (!this.IsPuppet() || !this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
            {
                this?.ApplyBodyState(state.BodyComponent);

                foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                        x.IsGenericType &&
                        x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
                {
                    var findIndex = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());
                    if (findIndex > -1)
                    {
                        if (state.NetworkComponents != null && state.NetworkComponents.ContainsKey(findIndex))
                        {
                            //     var decompose = state.Decompose();
                            var instanceMethod = component.GetType().GetMethod("ApplyNetworkState");
                            var parameterType = instanceMethod.GetParameters().First().ParameterType;

                            var methods = state.GetType().GetMethods();
                            var method = methods.Single(mi => mi.Name == "Decompose" && mi.GetParameters().Count() == 1);

                            var decomposed = method.MakeGenericMethod(parameterType)
                                  .Invoke(state, new object[] { findIndex });
                            instanceMethod.Invoke(component, new object[] { decomposed });
                        }
                    }
                }
            }
            else
            {
                while (stateQueue.Count >= 2)
                {
                    stateQueue.Dequeue();
                }
                stateQueue.Enqueue(state);
            }
        }

        internal void ProcessLocalInput()
        {
            float simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            var serverSendRate = simTickRate / 2;

            var MaxStaleServerStateTicks = (int)MathF.Ceiling(this.GameWorld.ServerVars.Get<int>("sv_max_stages_ms", 500) / serverSendRate);

            var camera = this.Components.Get<CharacterCamera>();
            var input = this.Components.Get<NetworkInput>();
            GeneralPlayerInput inputs = new GeneralPlayerInput();

            if (input != null && camera != null)
            {
                //set view rotation for input processor
                if (input.InputProcessor.InputEnabled && this.GameWorld.GameInstance.GuiDisableInput)
                    input.InputProcessor.InputEnabled = false;

                input.InputProcessor.ViewRotation = camera.GetViewRotation();
                inputs = input.InputProcessor.GetPlayerInput();
            }

            var lastTicks = GameWorld.WorldTick - (GameWorld as NetworkClientWorld).LastServerWorldTick;
            if (this.GameWorld.ServerVars.Get<bool>("sv_freze_client", false) && lastTicks >= MaxStaleServerStateTicks)
            {
                Logger.LogDebug(this, "Server state is too old (is the network connection dead?) - max ticks " + MaxStaleServerStateTicks + " - currentTicks => " + lastTicks);
                inputs = new GeneralPlayerInput();
            }

            // Update our snapshot buffers.
            uint bufidx = GameWorld.WorldTick % NetworkWorld.MaxTicks;
            this.localPlayerInputsSnapshots[bufidx] = inputs;
            this.localPlayerStateSnapshots[bufidx] = this.ToNetworkState();
            this.localPlayerWorldTickSnapshots[bufidx] = (GameWorld as NetworkClientWorld).LastServerWorldTick;

            // Send a command for all inputs not yet acknowledged from the server.
            var unackedInputs = new List<GeneralPlayerInput>();
            var clientWorldTickDeltas = new List<short>();

            // TODO: lastServerWorldTick is technically not the same as lastAckedInputTick, fix this.
            for (uint tick = (GameWorld as NetworkClientWorld).LastServerWorldTick; tick <= GameWorld.WorldTick; ++tick)
            {
                unackedInputs.Add(this.localPlayerInputsSnapshots[tick % NetworkWorld.MaxTicks]);
                clientWorldTickDeltas.Add((short)(tick - this.localPlayerWorldTickSnapshots[tick % NetworkWorld.MaxTicks]));
            }

            var command = new PlayerInputCommand
            {
                StartWorldTick = (GameWorld as NetworkClientWorld).LastServerWorldTick,
                Inputs = unackedInputs.ToArray(),
                ClientWorldTickDeltas = clientWorldTickDeltas.ToArray(),
            };

            // send to server => command
            (this.GameWorld as NetworkClientWorld).SendInputCommand(command);

            // SetPlayerInputs
            if (input != null)
                input.SetPlayerInputs(inputs);
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
                Logger.LogDebug(this, "RemotePlayer: not enough states to interp");
                return;
            }

            var nextState = stateQueue.Peek();
            float theta = stateTimer / ServerSendInterval;

            //only for movement component (to interpolate)
            var decomposedNextState = nextState.BodyComponent;
            var decomposesLastState = lastState.Value.BodyComponent;

            var a = decomposesLastState.Rotation;
            var b = decomposedNextState.Rotation;

            var newState = this.GetNetworkState();

            newState.Position = decomposesLastState.Position.Lerp(decomposedNextState.Position, theta);
            newState.Rotation = a.Slerp(b, theta);

            this.ApplyBodyState(newState);

            //for the rest -> just handle applying components
            foreach (var component in this.Components.All.Where(df => df.GetType().GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IChildNetworkSyncComponent<>))))
            {

                var index = this.AvaiablePlayerComponents.FindIndex(df => df.NodeType == component.GetType());
                if (index < 0)
                    continue;

                if (nextState.NetworkComponents.ContainsKey(index))
                {
                    var instanceMethod = component.GetType().GetMethod("ApplyNetworkState");
                    var parameterType = instanceMethod.GetParameters().First().ParameterType;

                    var methods = nextState.GetType().GetMethods();
                    var method = methods.Single(mi => mi.Name == "Decompose" && mi.GetParameters().Count() == 1);

                    var decomposed = method.MakeGenericMethod(parameterType)
                          .Invoke(nextState, new object[] { index });
                    instanceMethod.Invoke(component, new object[] { decomposed });
                }
            }
        }

        internal virtual void InternalTick(float delta)
        {
            if (this.teleportQueue.Count > 0)
            {
                var nextTeleport = this.teleportQueue.Dequeue();
                Logger.LogDebug(this, "Execute teleport to " + nextTeleport + " -> " + this.IsServer());

                this.MoveToPosition(nextTeleport);
            }

            if (this.IsLocal())
            {
                this.ProcessLocalInput();
            }

            //process server and client
            if (!this.IsPuppet())
            {
                var input = this.Components.Get<NetworkInput>();
                if (input != null)
                {
                    //   this.MovementProcessor.Velocity = this.Velocity;
                    this.MovementProcessor.SetServerVars(this.GameWorld.ServerVars);
                    this.MovementProcessor.SetClientVars(Framework.Game.Client.ClientSettings.Variables);
                    this.MovementProcessor.Simulate(this, input.LastInput, delta);
                }
            }
            //process puppet
            else
            {
                this.ProcessPuppetInput(delta);
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
        /// Rewind to an given world tick
        /// </summary>
        /// <param name="incomingStateTick"></param>
        /// <param name="headState"></param>
        public void Rewind(uint incomingStateTick, bool headState)
        {
            if (default(PlayerState).Equals(this.IncomingLocalPlayerState))
            {
                Logger.LogDebug(this, "No local player state found!");
            }

            // Lookup the historical state for the world tick we got.
            uint bufidx = incomingStateTick % NetworkWorld.MaxTicks;
            var stateSnapshot = this.localPlayerStateSnapshots[bufidx];

            // Compare the historical state to see how off it was.
            var incomingStateDecompose = this.IncomingLocalPlayerState.BodyComponent;
            var stateSnapshotDecompose = stateSnapshot.BodyComponent;

            var error = incomingStateDecompose.Position - stateSnapshotDecompose.Position;
            if (error.LengthSquared() > 0.0001f)
            {
                if (!headState)
                {
                    Logger.LogDebug(this, $"Rewind tick#{incomingStateTick}, Error: {error.Length()}, Range: {GameWorld.WorldTick - incomingStateTick} ClientPost: {incomingStateDecompose.Position.ToString()} ServerPos: {stateSnapshotDecompose.Position.ToString()} ");
                    replayedStates++;
                }

                // Rewind local player state to the correct state from the server.
                // TODO: Cleanup a lot of this when its merged with how rockets are spawned.
                this.ApplyNetworkState(this.IncomingLocalPlayerState);

                // Loop through and replay all captured input snapshots up to the current tick.
                uint replayTick = incomingStateTick;
                while (replayTick < GameWorld.WorldTick)
                {
                    // Grab the historical input.
                    bufidx = replayTick % NetworkWorld.MaxTicks;
                    var inputSnapshot = this.localPlayerInputsSnapshots[bufidx];

                    // Rewrite the historical sate snapshot.
                    this.localPlayerStateSnapshots[bufidx] = this.ToNetworkState();

                    var input = this.Components.Get<NetworkInput>();

                    // Apply inputs to the associated player controller and simulate the world.
                    input?.SetPlayerInputs(inputSnapshot);
                    this.InternalTick((float)this.GetPhysicsProcessDeltaTime());

                    ++replayTick;
                }
            }
        }

        /// <inheritdoc /> 
        public void AddAvaiableComponent<T>(string ResourcePath = null, string ScriptPath = null)
        where T : Godot.Node, IPlayerComponent, new()
        {
            var element = new AssignedComponent(
                                typeof(T), ResourcePath, ScriptPath
                );


            if (!this.avaiableComponents.Contains(element))
            {
                this.avaiableComponents.Add(element);
            }
        }
        private Queue<Vector3> teleportQueue = new Queue<Vector3>();

        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void DoTeleport(Godot.Vector3 origin)
        {
            Logger.LogDebug(this, "Enquene teleport to " + origin);
            this.teleportQueue.Enqueue(origin);
        }

        /// <summary>
        /// Add an assigned network component
        /// </summary>
        /// <param name="assignedComponent"></param>
        public void AddAssignedComponent(AssignedComponent assignedComponent)
        {
            if (assignedComponent.ResourcePath != null)
            {
                Logger.LogDebug(this, "Try to load component from type " + assignedComponent.NodeType.Name);
                //  this.Components.AddComponent(assignedComponent.NodeType, assignedComponent.ResourcePath);

                if (assignedComponent.ScriptPath != null)
                {
                    Godot.GD.Load<Godot.CSharpScript>(assignedComponent.ScriptPath);
                }

                this.Components.AddComponentAsync(assignedComponent.NodeType, assignedComponent.ResourcePath, (Godot.Node res) =>
                {
                    Logger.LogDebug(this, "Add component from type " + assignedComponent.NodeType.Name);
                });
            }
            else
            {
                this.Components.AddComponent(assignedComponent.NodeType);
                Logger.LogDebug(this, "Add component from type " + assignedComponent.NodeType.Name);
            }
        }
    }
}