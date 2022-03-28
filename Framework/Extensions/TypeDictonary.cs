
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Extensions
{
    /// <summary>
    /// TypeDictonary contains an generic type as key and an generic object as value
    /// </summary>
    public class TypeDictonary<T2>
    {
        private readonly Dictionary<Type, T2> _values = new();

        /// <summary>
        /// Get a full list of registered services
        /// </summary>
        /// <returns></returns>
        public T2[] All => _values.Values.ToArray();


        /// <summary>
        /// Create and register service by given type
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        public T Create<T>() where T : class, T2
        {
            lock (_values)
            {
                T newService = Activator.CreateInstance<T>();
                _values.Add(typeof(T), newService);

                return newService;
            }
        }

        /// <inheritdoc />
        private bool TryGetService(Type type, out T2 service)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            lock (_values)
            {
                return _values.TryGetValue(type, out service);
            }
        }

        /// <summary>
        /// Get an registered service by given type
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        public T Get<T>() where T : class, T2
        {
            if (!TryGetService(typeof(T), out T2 service))
            {
                return null;
            }

            return (T)service;
        }
    }
}

