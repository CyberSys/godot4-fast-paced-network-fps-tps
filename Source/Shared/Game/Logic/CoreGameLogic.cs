using Godot;

namespace Shooter.Shared
{
    public abstract partial class CoreGameLogic : SubViewport, IGameLogic
    {
        /// <inheritdoc />
        private readonly ServiceRegistry _serviceRegistry = new ServiceRegistry();

        /// <inheritdoc />
        private bool mapLoadInProcess = false;

        /// <summary>
        /// Instance an map by given PackedScene and WorldTick
        /// </summary>
        protected abstract void OnMapInstance(PackedScene level, uint worldTick);

        /// <summary>
        /// On destroy map
        /// </summary>
        protected abstract void OnMapDestroy();

        /// <inheritdoc />
        protected IWorld currentWorld;

        /// <summary>
        /// Secure passphrase for network connection
        /// </summary>
        protected readonly string secureConnectionKey = "ConnectionKey";

        /// <summary>
        /// The async loader for eg. maps
        /// </summary>
        /// <returns></returns>
        public AsyncLoader loader = new AsyncLoader();

        /// <summary>
        /// Service Registry (Contains all services)
        /// </summary>
        public ServiceRegistry Services => _serviceRegistry;

        /// <summary>
        /// The active game world
        /// </summary>
        public IWorld CurrentWorld => currentWorld;

        /// <inheritdoc />
        public CoreGameLogic() : base()
        {
            this.OwnWorld3d = true;
            this.RenderTargetUpdateMode = UpdateMode.Always;
            this.RenderTargetClearMode = ClearMode.Always;
            this.ProcessMode = ProcessModeEnum.Always;
        }

        /// <summary>
        /// On tree enter
        /// </summary>
        public override void _EnterTree()
        {
            base._EnterTree();

            var origSize = this.GetParent<SubViewportContainer>().Size;
            this.Size = new Vector2i((int)origSize.x, (int)origSize.y);

            Logger.LogDebug(this, "Service amount: " + _serviceRegistry.All.Length);

            foreach (var service in _serviceRegistry.All)
            {
                service.Register();
            }
        }

        /// <summary>
        /// Update all regisered services
        /// </summary>
        /// <param name="delta"></param>
        public override void _Process(float delta)
        {
            base._Process(delta);

            foreach (var service in _serviceRegistry.All)
            {
                service.Update(delta);
            }

            this.loader.Tick();
        }

        /// <summary>
        /// After exit tree
        /// </summary>
        public override void _ExitTree()
        {
            foreach (var service in _serviceRegistry.All)
            {
                service.Unregister();
            }

            base._ExitTree();
        }

        /// <summary>
        /// Load an world by given resource path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="worldTick"></param>
        protected virtual void LoadWorld(string path, uint worldTick = 0)
        {
            if (mapLoadInProcess)
            {
                Logger.LogDebug(this, "There is currently an map in progress.");
                return;
            }

            this.OnMapDestroy();
            this.mapLoadInProcess = true;

            GD.Load<CSharpScript>("res://Shared/Game/GameLevel.cs");

            this.loader.LoadResource(path, (res) =>
            {
                this.mapLoadInProcess = false;
                this.OnMapInstance((PackedScene)res, worldTick);
            });
        }
    }
}
