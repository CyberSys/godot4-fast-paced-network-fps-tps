using Godot;

namespace Shooter.Shared
{
    public abstract partial class CoreGameLogic : SubViewport, IGameLogic
    {
        protected readonly ServiceRegistry _serviceRegistry = new ServiceRegistry();
        public ServiceRegistry Services => _serviceRegistry;
        private bool mapLoadInProcess = false;
        protected abstract void OnMapInstance(PackedScene level, uint worldTick);
        protected abstract void OnMapDestroy();
        protected IWorld currentWorld;
        public IWorld CurrentWorld => currentWorld;
        public AsyncLoader loader = new AsyncLoader();

        public CoreGameLogic() : base()
        {
            this.OwnWorld3d = true;
            this.RenderTargetUpdateMode = UpdateMode.Always;
            this.RenderTargetClearMode = ClearMode.Always;
            this.ProcessMode = ProcessModeEnum.Always;
        }

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

        public override void _Process(float delta)
        {
            base._Process(delta);

            foreach (var service in _serviceRegistry.All)
            {
                service.Update(delta);
            }

            this.loader.Tick();
        }

        public override void _ExitTree()
        {
            foreach (var service in _serviceRegistry.All)
            {
                service.Unregister();
            }

            base._ExitTree();
        }

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
