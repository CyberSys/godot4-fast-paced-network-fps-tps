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
using System;
using System.Linq;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// Component registry class
    /// </summary>
    public class ComponentRegistry<T> where T : IBaseComponent
    {
        /// <inheritdoc />
        private T baseComponent;

        /// <summary>
        /// Get all avaiable components
        /// </summary>
        /// <returns></returns>
        public Node[] All
        {
            get
            {
                var list = new List<Node>();
                foreach (var element in this.baseComponent.GetChildren())
                {
                    if (element is IChildComponent<T> && element is Node)
                    {
                        list.Add(element as Node);
                    }
                }

                return list.ToArray();
            }
        }

        /// <summary>
        /// Triggered when an component fully added
        /// </summary>
        [Godot.Signal]
        public event OnComponentAddedHandler OnComponentAdded;

        /// <summary>
        /// The event handler for adding components
        /// </summary>
        /// <param name="component"></param>
        public delegate void OnComponentAddedHandler(Godot.Node component);

        /// <summary>
        /// Delete all exist components and remove them from base component
        /// </summary>
        public void Clear()
        {
            foreach (var comp in this.All)
            {
                if (comp.IsInsideTree())
                {
                    this.baseComponent.RemoveChild(comp);
                }
            }
        }

        /// <summary>
        /// Create a component registry by given base component
        /// </summary>
        /// <param name="baseComponent">The base component</param>
        public ComponentRegistry(T baseComponent)
        {
            this.baseComponent = baseComponent;
        }

        /// <summary>
        /// Delete a given componentn from base component
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        public void DeleteComponent<T2>() where T2 : Node, IChildComponent<T>
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == typeof(T2));
            if (element != null)
            {
                if (element.IsInsideTree())
                {
                    this.baseComponent.RemoveChild(element);
                }
            }
        }

        /// <summary>
        /// Delete a given componentn from base component by given type
        /// </summary>
        /// <param name="componentType">Type of component which have to be deleted</param>
        /// 
        public void DeleteComponent(Type componentType)
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == componentType);
            if (element != null)
            {
                if (element.IsInsideTree())
                {
                    this.baseComponent.RemoveChild(element);
                }
            }
        }

        //required for fix multiple loads in async
        private List<string> onHold = new List<string>();

        /// <summary>
        /// Add an new component to base component by given resource path (async)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="resourcePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void AddComponentAsync(Type type, string resourcePath, Action<Node> callback)
        {
            lock (onHold)
            {
                if (!this.onHold.Contains(resourcePath))
                {
                    this.onHold.Add(resourcePath);

                    Framework.Utils.AsyncLoader.Loader.LoadResource(resourcePath, (res) =>
                    {
                        var result = AddComponent(type, (PackedScene)res);
                        if (callback != null)
                        {
                            callback(result);
                        }

                        this.onHold.Remove(resourcePath);
                    });
                }
            }
        }

        /// <summary>
        /// Add an new component to base component
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 AddComponent<T2>() where T2 : Node, IChildComponent<T>
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == typeof(T2));
            if (element == null)
            {
                T2 component = Activator.CreateInstance<T2>();

                component.Name = typeof(T2).Name;
                component.BaseComponent = this.baseComponent;

                if (this.baseComponent.IsInsideTree())
                {
                    this.baseComponent.AddChild(component);
                }

                this.OnComponentAdded?.Invoke(component);
                return component;
            }
            else
            {
                Logger.LogDebug(this, "An node from this type already exist");
            }

            return null;
        }

        /// <summary>
        /// Add an new component to base component
        /// </summary>
        /// <param name="type">Type of the component which have to be added</param>
        /// <returns></returns>
        public Node AddComponent(Type type)
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == type);
            if (element == null)
            {
                object createdObject = Activator.CreateInstance(type);
                if (createdObject is IChildComponent<T> && createdObject is Node)
                {
                    (createdObject as Node).Name = type.Name;
                    (createdObject as IChildComponent<T>).BaseComponent = this.baseComponent;

                    if (this.baseComponent.IsInsideTree())
                    {
                        this.baseComponent.AddChild(createdObject as Node);
                    }

                    this.OnComponentAdded?.Invoke(createdObject as Node);
                    return createdObject as Node;
                }
            }
            return null;
        }

        /// <summary>
        /// Add an new component to base component by given resource name
        /// </summary>
        /// <param name="type">Type of the component which have to be added</param>
        /// <param name="resourcePath">Path to the godot resource</param>
        /// <returns></returns>
        public Node AddComponent(Type type, string resourcePath)
        {
            var scene = GD.Load<PackedScene>(resourcePath);
            return this.AddComponent(type, scene);
        }


        /// <summary>
        /// /// Add component as packed scene
        /// </summary>
        /// <param name="type"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        public Node AddComponent(Type type, PackedScene scene)
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == type);
            if (element == null)
            {
                //scene.ResourceLocalToScene = true;
                var createdObject = scene.Instantiate();
                if (createdObject is IChildComponent<T> && createdObject is Node)
                {
                    (createdObject as Node).Name = type.Name;
                    (createdObject as IChildComponent<T>).BaseComponent = this.baseComponent;

                    if (this.baseComponent.IsInsideTree())
                    {
                        this.baseComponent.AddChild(createdObject as Node);
                    }

                    this.OnComponentAdded?.Invoke(createdObject as Node);
                    return createdObject as Node;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if component exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasComponent(Type type)
        {
            return this.All.Count(df => df.GetType() == type) > 0;
        }

        /// <summary>
        /// Add an new component to base component by given resource path
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 AddComponent<T2>(string resourcePath) where T2 : Node, IChildComponent<T>
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == typeof(T2));
            if (element != null)
            {
                this.DeleteComponent<T2>();
            }

            var scene = GD.Load<PackedScene>(resourcePath);
            //  scene.ResourceLocalToScene = true;
            var component = scene.Instantiate<T2>();
            component.Name = typeof(T2).Name;
            component.BaseComponent = this.baseComponent;

            if (this.baseComponent.IsInsideTree())
            {
                this.baseComponent.AddChild(component);
            }

            this.OnComponentAdded?.Invoke(component);
            return component;
        }

        /// <summary>
        /// Get an existing component of the base component
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public T2 Get<T2>() where T2 : Node, IChildComponent<T>
        {
            var element = this.All.FirstOrDefault(df => df is T2);
            if (element != null)
            {
                return element as T2;
            }

            return null;
        }

        /// <summary>
        /// Get an existing component of the base component
        /// </summary>
        /// <returns></returns>
        public object Get(Type t)
        {
            var element = this.All.FirstOrDefault(df => df.GetType() == t);
            if (element != null)
            {
                return element;
            }

            return null;
        }
    }
}