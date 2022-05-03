using System;
using System.Collections.Generic;
using Godot;

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

namespace Framework.Utils
{
    /// <summary>
    /// Helper class to load resources in background
    /// </summary>
    public class AsyncLoader
    {
        /// <summary>
        /// The static async loader
        /// </summary>
        /// <returns></returns>
        public static Framework.Utils.AsyncLoader Loader { get; private set; } = new Utils.AsyncLoader();

        /// <inheritdoc/>
        internal class LoadingRequest
        {
            public string ResourceName { get; set; }
            public Action<Resource> OnSucess { get; set; }
        }

        [Signal]
        public event ProgressHandler OnProgress;
        public delegate void ProgressHandler(string filename, float percent);

        private Queue<LoadingRequest> resourceLoader = new Queue<LoadingRequest>();

        private LoadingRequest currentResource = null;

        /// <summary>
        /// Load an resource by given path
        /// </summary>
        /// <param name="resourceName">Path to resource file</param>
        /// <param name="callback">An action with returning an Resource/param>
        /// <returns></returns>
        public void LoadResource(string resourceName, Action<Resource> callback)
        {
            Logger.LogDebug(this, "Try to start load  " + resourceName);

            this.resourceLoader.Enqueue(new LoadingRequest
            {
                ResourceName = resourceName,
                OnSucess = callback
            });
        }

        /// <summary>
        /// Brings the async loader to the next state.
        /// Binded on your _Process Method
        /// </summary>
        public void Tick()
        {
            if (resourceLoader.Count > 0 && this.currentResource == null)
            {
                this.currentResource = resourceLoader.Dequeue();

                var result = ResourceLoader.LoadThreadedRequest(currentResource.ResourceName);
                if (result != Error.Ok)
                {
                    Logger.LogDebug(this, "Cant load resource " + currentResource.ResourceName + " - Reason: " + result.ToString());
                    this.currentResource = null;
                }
                else
                {
                    this.OnProgress?.Invoke(currentResource.ResourceName, 0);
                }
            }

            this.CheckLoad();
        }

        private void CheckLoad()
        {
            if (this.currentResource != null)
            {
                var mapLoaderProgress = new Godot.Collections.Array();
                if (currentResource != null)
                {
                    var status = ResourceLoader.LoadThreadedGetStatus(currentResource.ResourceName, mapLoaderProgress);
                    if (status == ResourceLoader.ThreadLoadStatus.Loaded)
                    {
                        Resource res = ResourceLoader.LoadThreadedGet(currentResource.ResourceName);
                        Logger.LogDebug(this, "Resource loaded " + currentResource.ResourceName);

                        if (currentResource.OnSucess != null)
                        {
                            currentResource.OnSucess(res);
                        }

                        currentResource = null;
                    }
                    else if (status == ResourceLoader.ThreadLoadStatus.InvalidResource || status == ResourceLoader.ThreadLoadStatus.Failed)
                    {
                        Logger.LogDebug(this, "Error loading  " + currentResource.ResourceName);
                        currentResource = null;
                    }
                    else if (status == ResourceLoader.ThreadLoadStatus.InProgress)
                    {
                        if (mapLoaderProgress.Count > 0)
                        {
                            this.OnProgress?.Invoke(currentResource.ResourceName, (float)mapLoaderProgress[0]);
                        }
                    }
                }
            }
        }

    }
}
