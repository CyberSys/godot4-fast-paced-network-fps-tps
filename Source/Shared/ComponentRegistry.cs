using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Shooter.Shared
{
    public class ComponentRegistry<T2> where T2 : Node
    {
        private readonly Dictionary<Type, Node> _components = new();

        public Node[] All => _components.Values.ToArray();
        private T2 parentNode = null;

        /// <inheritdoc />
        public ComponentRegistry(T2 node)
        {
            this.parentNode = node;
            this.parentNode.TreeEntered += () =>
            {
                foreach (var comp in _components)
                {
                    if (!comp.Value.IsInsideTree())
                    {
                        this.parentNode.AddChild(comp.Value);
                    }
                }
            };
        }

        /// <inheritdoc />
        public void DeleteComponent<T>()
        {
            lock (_components)
            {
                if (this._components.ContainsKey(typeof(T)))
                {
                    if (this._components[typeof(T)].IsInsideTree())
                    {
                        this.parentNode.RemoveChild(this._components[typeof(T)]);
                    }
                    this._components.Remove(typeof(T));
                }
            }
        }

        /// <inheritdoc />
        public T AddComponent<T>() where T : Node, IComponent<T2>
        {
            lock (_components)
            {
                T component = Activator.CreateInstance<T>();
                _components[typeof(T)] = component;
                component.Name = typeof(T).Name;
                component.MainComponent = this.parentNode;

                if (this.parentNode.IsInsideTree())
                {
                    this.parentNode.AddChild(component);
                }

                return component;
            }
        }

        /// <inheritdoc />
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
                component.MainComponent = this.parentNode;

                if (this.parentNode.IsInsideTree())
                {
                    this.parentNode.AddChild(component);
                }

                return component;
            }
        }

        /// <inheritdoc />
        public bool TryGetService(Type type, out Node service)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            lock (_components)
            {
                return _components.TryGetValue(type, out service);
            }
        }

        /// <inheritdoc />
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