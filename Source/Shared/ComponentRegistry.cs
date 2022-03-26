using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Shooter.Shared
{
    public class ComponentRegistry<T2> where T2 : Node
    {
        private readonly Dictionary<Type, Node> _components = new();

        /// <summary>
        /// Get all avaiable components
        /// </summary>
        /// <returns></returns>
        public Node[] All => _components.Values.ToArray();


        private T2 baseComponent = null;

        /// <summary>
        /// Create a component registry by given base component
        /// </summary>
        /// <param name="node"></param>
        public ComponentRegistry(T2 baseComponent)
        {
            this.baseComponent = baseComponent;
            this.baseComponent.TreeEntered += () =>
            {
                foreach (var comp in _components)
                {
                    if (!comp.Value.IsInsideTree())
                    {
                        this.baseComponent.AddChild(comp.Value);
                    }
                }
            };
        }

        /// <summary>
        /// Delete a given componentn from base component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteComponent<T>()
        {
            lock (_components)
            {
                if (this._components.ContainsKey(typeof(T)))
                {
                    if (this._components[typeof(T)].IsInsideTree())
                    {
                        this.baseComponent.RemoveChild(this._components[typeof(T)]);
                    }
                    this._components.Remove(typeof(T));
                }
            }
        }

        /// <summary>
        /// Add an new component to base component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : Node, IComponent<T2>
        {
            lock (_components)
            {
                T component = Activator.CreateInstance<T>();
                _components[typeof(T)] = component;
                component.Name = typeof(T).Name;
                component.MainComponent = this.baseComponent;

                if (this.baseComponent.IsInsideTree())
                {
                    this.baseComponent.AddChild(component);
                }

                return component;
            }
        }

        /// <summary>
        /// Add an new component to base component by given resource path (async)
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void AddComponentAsync<T>(string resourcePath, Action<bool> callback) where T : Node, IComponent<T2>
        {

        }

        /// <summary>
        /// Add an new component to base component by given resource path
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>(string resourcePath) where T : Node, IComponent<T2>
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
                component.MainComponent = this.baseComponent;

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
        public T? Get<T>() where T : Node, IComponent<T2>
        {
            if (!TryGetService(typeof(T), out Node service))
            {
                return null;
            }

            return (T)service;
        }
    }
}