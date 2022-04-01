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
    public class ComponentRegistry
    {
        /// <inheritdoc />
        private readonly Dictionary<Type, Node> _components = new();

        /// <inheritdoc />
        private IBaseComponent baseComponent = null;

        /// <summary>
        /// Get all avaiable components
        /// </summary>
        /// <returns></returns>
        public Node[] All => _components.Values.ToArray();


        /// <summary>
        /// Delete all exist components and remove them from base component
        /// </summary>
        public void Clear()
        {
            lock (_components)
            {
                foreach (var comp in this._components.ToArray())
                {
                    if (comp.Value.IsInsideTree())
                    {
                        this.baseComponent.RemoveChild(comp.Value);
                    }

                    this._components.Remove(comp.Key);
                }
            }
        }

        /// <summary>
        /// Create a component registry by given base component
        /// </summary>
        /// <param name="baseComponent">The base component</param>
        public ComponentRegistry(IBaseComponent baseComponent)
        {
            this.baseComponent = baseComponent;
            this.baseComponent.TreeEntered += () =>
            {
                foreach (var comp in _components)
                {
                    if (comp.Value is Node)
                    {
                        var node = comp.Value as Node;
                        if (!node.IsInsideTree())
                        {
                            this.baseComponent.AddChild(node);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Delete a given componentn from base component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteComponent<T>() where T : Node, IChildComponent
        {
            lock (_components)
            {
                if (this._components.ContainsKey(typeof(T)))
                {
                    var node = this._components[typeof(T)];

                    if (node.IsInsideTree())
                    {
                        this.baseComponent.RemoveChild(node);
                    }

                    this._components.Remove(typeof(T));
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
            lock (_components)
            {
                if (this._components.ContainsKey(componentType))
                {
                    var node = this._components[componentType];

                    if (node.IsInsideTree())
                    {
                        this.baseComponent.RemoveChild(node);
                    }

                    this._components.Remove(componentType);
                }
            }
        }


        /// <summary>
        /// Add an new component to base component by given resource path (async)
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void AddComponentAsync<T>(string resourcePath, Action<bool> callback) where T : IChildComponent
        {

        }

        /// <summary>
        /// Add an new component to base component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : Node, IChildComponent
        {
            lock (_components)
            {
                T component = Activator.CreateInstance<T>();

                _components[typeof(T)] = component;

                component.Name = typeof(T).Name;
                component.BaseComponent = this.baseComponent;

                if (this.baseComponent.IsInsideTree())
                {
                    this.baseComponent.AddChild(component);
                }

                return component;
            }
        }

        /// <summary>
        /// Add an new component to base component
        /// </summary>
        /// <param name="type">Type of the component which have to be added</param>
        /// <returns></returns>
        public Node AddComponent(Type type)
        {
            lock (_components)
            {
                object createdObject = Activator.CreateInstance(type);
                if (createdObject is IChildComponent && createdObject is Node)
                {
                    _components[type] = createdObject as Node;

                    (createdObject as Node).Name = type.Name;
                    (createdObject as IChildComponent).BaseComponent = this.baseComponent;

                    if (this.baseComponent.IsInsideTree())
                    {
                        this.baseComponent.AddChild(createdObject as Node);
                    }

                    return createdObject as Node;
                }
                else return null;
            }
        }

        /// <summary>
        /// Add an new component to base component by given resource name
        /// </summary>
        /// <param name="type">Type of the component which have to be added</param>
        /// <param name="resourcePath">Path to the godot resource</param>
        /// <returns></returns>
        public Node AddComponent(Type type, string resourcePath)
        {
            lock (_components)
            {
                var scene = GD.Load<PackedScene>(resourcePath);
                scene.ResourceLocalToScene = true;
                var createdObject = scene.Instantiate();
                if (createdObject is IChildComponent && createdObject is Node)
                {
                    _components[type] = createdObject as Node;

                    (createdObject as Node).Name = (createdObject as IChildComponent).GetComponentName();
                    (createdObject as IChildComponent).BaseComponent = this.baseComponent;

                    if (this.baseComponent.IsInsideTree())
                    {
                        this.baseComponent.AddChild(createdObject as Node);
                    }

                    return createdObject as Node;
                }
                else return null;
            }
        }

        /// <summary>
        /// Check if component exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasComponent(Type type)
        {
            return this._components.ContainsKey(type);
        }

        /// <summary>
        /// Add an new component to base component by given resource path
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>(string resourcePath) where T : Node, IChildComponent
        {
            lock (_components)
            {
                if (this._components.ContainsKey(typeof(T)))
                {
                    this.DeleteComponent<T>();
                }

                var scene = GD.Load<PackedScene>(resourcePath);
                scene.ResourceLocalToScene = true;
                var component = scene.Instantiate<T>();

                _components[typeof(T)] = component;
                component.Name = typeof(T).Name;
                component.BaseComponent = this.baseComponent;

                if (this.baseComponent.IsInsideTree())
                {
                    this.baseComponent.AddChild(component);
                }

                return component;
            }
        }

        /// <inheritdoc />
        private bool TryGetService(Type type, out Node service)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            lock (_components)
            {
                return _components.TryGetValue(type, out service);
            }
        }

        /// <summary>
        /// Get an existing component of the base component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : Node, IChildComponent
        {
            if (!TryGetService(typeof(T), out Node service))
            {
                return null;
            }

            return (T)service;
        }

    }
}