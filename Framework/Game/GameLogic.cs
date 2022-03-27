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

        /// <summary>
        /// Instance an map by given PackedScene and WorldTick
        /// </summary>
        protected virtual void OnMapInstance(PackedScene level, uint worldTick)
        {

        }

        /// <summary>
        /// On destroy map
        /// </summary>
        protected virtual void OnMapDestroy()
        {

        }


        /// <inheritdoc />
        private AsyncLoader loader = new AsyncLoader();

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

            this.loader.LoadResource(path, (res) =>
            {
                this.mapLoadInProcess = false;
                this.OnMapInstance((PackedScene)res, worldTick);
            });
        }

        public virtual void AfterMapInstance()
        {

        }

        public virtual void AfterMapDestroy()
        {

        }
        public virtual void AfterInit()
        {

        }
    }
}
