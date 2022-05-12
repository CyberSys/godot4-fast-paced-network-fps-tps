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
using System;
using System.Diagnostics;

namespace Framework.Game
{
    /// <summary>
    /// Basic game logic component
    /// </summary>
    public partial class GameLogic : SubViewport, IGameLogic
    {
        /// <inheritdoc />
        public ComponentRegistry<GameLogic> Components => _components;
        private ComponentRegistry<GameLogic> _components;

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
            if (what == (int)Godot.Node.NotificationReady)
            {
                this.InternalReady();
            }

            if (what == (int)Godot.Node.NotificationExitTree)
            {
                this.InternalTreeExit();
            }
        }

        //prevent to load async loader for multiply game logics
        internal static bool resourceLoaderTick = false;

        private bool canRunAysncLoader = false;

        /// <inheritdoc />
        public override void _Process(float delta)
        {
            this.InternalProcess(delta);
        }

        /// <inheritdoc />
        internal virtual void OnMapInstanceInternal(PackedScene level, string scriptPath, uint worldTick)
        {

        }

        /// <inheritdoc />
        internal virtual void DestroyMapInternal()
        {

        }

        /// <inheritdoc />
        protected INetworkWorld currentWorld;

        /// <summary>
        /// Secure passphrase for network connection
        /// </summary>
        public string secureConnectionKey = "ConnectionKey";


        /// <summary>
        /// Service Registry (Contains all services)
        /// </summary>
        public TypeDictonary<IService> Services => _serviceRegistry;

        /// <summary>
        /// The active game world
        /// </summary>
        public INetworkWorld CurrentWorld => currentWorld;

        /// <inheritdoc />
        public GameLogic() : base()
        {
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            // Of course this only affects the main thread rather than child threads.
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            this.OwnWorld3d = true;
            this.RenderTargetUpdateMode = UpdateMode.Always;
            this.RenderTargetClearMode = ClearMode.Always;
            this.ProcessMode = ProcessModeEnum.Always;
            this.Scaling3dMode = Scaling3DMode.Fsr;

            this._components = new ComponentRegistry<GameLogic>(this);

            if (!resourceLoaderTick)
            {
                resourceLoaderTick = true;
                canRunAysncLoader = true;
            }
        }

        internal virtual void InternalReady()
        {

        }

        /// <summary>
        /// On tree enter
        /// </summary>
        internal virtual void InternalTreeEntered()
        {

            var parent = this.GetParentOrNull<Control>();
            if (parent != null)
            {
                parent.ProcessMode = ProcessModeEnum.Always;
                Logger.LogDebug(this, "Found parent set size: " + parent.Size);
                this.Size = new Vector2i((int)parent.Size.x, (int)parent.Size.y);
            }

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

            // Prevent to call the async loader twice!
            if (canRunAysncLoader)
                Framework.Utils.AsyncLoader.Loader.Tick();
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
        /// <param name="scriptPath"></param>
        /// <param name="worldTick"></param>
        /// 
        public void LoadWorld(string path, string scriptPath, uint worldTick = 0)
        {
            this.LoadWorldInternal(path, scriptPath, worldTick);
        }

        /// <inheritdoc />
        internal virtual void LoadWorldInternal(string path, string scriptPath, uint worldTick = 0)
        {
            if (mapLoadInProcess)
            {
                Logger.LogDebug(this, "There is currently an map in progress.");
                return;
            }

            GD.Load<CSharpScript>(scriptPath);

            this.DestroyMapInternal();
            this.mapLoadInProcess = true;

            Framework.Utils.AsyncLoader.Loader.LoadResource(path, (res) =>
             {
                 this.mapLoadInProcess = false;
                 this.OnMapInstanceInternal((PackedScene)res, scriptPath, worldTick);
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
