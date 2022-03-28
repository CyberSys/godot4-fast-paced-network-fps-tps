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

using Godot;
using Framework.Utils;
using Framework.Extensions;

namespace Framework.Game
{
    /// <summary>
    /// Basic game logic component
    /// </summary>
    public partial class GameLogic : SubViewport, IGameLogic, IBaseComponent
    {
        /// <inheritdoc />
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

        /// <summary>
        /// Call when an map loaded succesfully
        /// </summary>
        public virtual void AfterMapLoaded()
        {

        }

        /// <summary>
        /// Called when an map unloaded
        /// </summary>
        public virtual void AfterMapDestroy()
        {

        }
    }
}
