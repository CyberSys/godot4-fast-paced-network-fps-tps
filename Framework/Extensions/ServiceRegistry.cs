
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework
{
    /// <summary>
    /// Service registry /service holder of component
    /// </summary>
    public class ServiceRegistry
    {
        private readonly Dictionary<Type, IService> _registeredServices = new();

        /// <summary>
        /// Get a full list of registered services
        /// </summary>
        /// <returns></returns>
        public IService[] All => _registeredServices.Values.ToArray();


        /// <summary>
        /// Create and register service by given type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        public T Create<T>() where T : class, IService
        {
            lock (_registeredServices)
            {
                T newService = Activator.CreateInstance<T>();
                _registeredServices.Add(typeof(T), newService);

                return newService;
            }
        }

        /// <inheritdoc />
        private bool TryGetService(Type type, out IService service)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            lock (_registeredServices)
            {
                return _registeredServices.TryGetValue(type, out service);
            }
        }

        /// <summary>
        /// Get an registered service by given type
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        public T? Get<T>() where T : class, IService
        {
            if (!TryGetService(typeof(T), out IService service))
            {
                return null;
            }

            return (T)service;
        }
    }
}

