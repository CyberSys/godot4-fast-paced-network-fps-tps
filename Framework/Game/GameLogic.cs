using Godot;
using Framework.Utils;

namespace Framework.Game
{
    /// <summary>
    /// Basic game logic component
    /// </summary>
    public partial class GameLogic : SubViewport, IGameLogic, IBaseComponent
    {
        public ComponentRegistry Components => _components;
        private ComponentRegistry _components;

        /// <inheritdoc />
        private readonly TypeDictonary<IService> _serviceRegistry = new TypeDictonary<IService>();

        /// <inheritdoc />
        private bool mapLoadInProcess = false;

        /// <inheritdoc />
        public override void _Notification(int what)
        {
            if (what == (int)Godot.Node.NotificationEnterTree)
            {
                this.InternalTreeEntered();
            }

            if (what == (int)Godot.Node.NotificationExitTree)
            {
                this.InternalTreeExit();
            }
        }

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            this.InternalProcess(delta);
        }

        /// <inheritdoc />
        internal virtual void OnMapInstanceInternal(PackedScene level, uint worldTick)
        {

        }

        /// <inheritdoc />
        internal virtual void DestroyMapInternal()
        {

        }


        /// <inheritdoc />
        private AsyncLoader loader = new AsyncLoader();

        /// <inheritdoc />
        protected IWorld currentWorld;

        /// <summary>
        /// Secure passphrase for network connection
        /// </summary>
        public string secureConnectionKey = "ConnectionKey";

        /// <summary>
        /// The async loader for eg. maps
        /// </summary>
        /// <returns></returns>        
        public AsyncLoader MapLoader => loader;

        /// <summary>
        /// Service Registry (Contains all services)
        /// </summary>
        public TypeDictonary<IService> Services => _serviceRegistry;

        /// <summary>
        /// The active game world
        /// </summary>
        public IWorld CurrentWorld => currentWorld;

        /// <inheritdoc />
        public GameLogic() : base()
        {
            this.OwnWorld3d = true;
            this.RenderTargetUpdateMode = UpdateMode.Always;
            this.RenderTargetClearMode = ClearMode.Always;
            this.ProcessMode = ProcessModeEnum.Always;

            this._components = new ComponentRegistry(this);
        }



        /// <summary>
        /// On tree enter
        /// </summary>
        internal virtual void InternalTreeEntered()
        {
            var origSize = this.GetParent<SubViewportContainer>().Size;
            this.Size = new Vector2i((int)origSize.x, (int)origSize.y);

            Logger.LogDebug(this, "Service amount: " + _serviceRegistry.All.Length);

            foreach (var service in _serviceRegistry.All)
            {
                service.Register();
            }
        }


        /// <inheritdoc />
        internal virtual void InternalProcess(float delta)
        {
            foreach (var service in _serviceRegistry.All)
            {
                service.Update(delta);
            }

            this.loader.Tick();
        }

        /// <inheritdoc />
        internal virtual void InternalTreeExit()
        {
            foreach (var service in _serviceRegistry.All)
            {
                service.Unregister();
            }
        }

        /// <summary>
        /// Load an world by given resource path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="worldTick"></param>
        /// 
        public void LoadWorld(string path, uint worldTick = 0)
        {
            this.LoadWorldInternal(path, worldTick);
        }

        /// <inheritdoc />
        internal virtual void LoadWorldInternal(string path, uint worldTick = 0)
        {
            if (mapLoadInProcess)
            {
                Logger.LogDebug(this, "There is currently an map in progress.");
                return;
            }

            this.DestroyMapInternal();
            this.mapLoadInProcess = true;

            this.loader.LoadResource(path, (res) =>
            {
                this.mapLoadInProcess = false;
                this.OnMapInstanceInternal((PackedScene)res, worldTick);
            });
        }

        public virtual void AfterMapInstance()
        {

        }

        public virtual void AfterMapDestroy()
        {

        }
    }
}
