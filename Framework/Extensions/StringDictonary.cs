
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Extensions
{
    /// <summary>
    /// Service registry /service holder of component
    /// </summary>
    public class StringDictonary<T2>
    {
        private readonly Dictionary<string, T2> _registeredServices = new();

        /// <summary>
        /// Get a full list of registered services
        /// </summary>
        /// <returns></returns>
        public T2[] All => _registeredServices.Values.ToArray();

        /// <summary>
        /// Create and register service by given type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        public T Create<T>(string name) where T : class, T2
        {
            lock (_registeredServices)
            {
                T newService = Activator.CreateInstance<T>();
                _registeredServices.Add(name, newService);

                return newService;
            }
        }

        /// <inheritdoc />
        private bool TryGetService(string name, out T2 service)
        {
            lock (_registeredServices)
            {
                return _registeredServices.TryGetValue(name, out service);
            }
        }

        /// <summary>
        /// Get an registered service by given type
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        public T? Get<T>(string name) where T : class, T2
        {
            if (!TryGetService(name, out T2 service))
            {
                return null;
            }

            return (T)service;
        }
    }
}

