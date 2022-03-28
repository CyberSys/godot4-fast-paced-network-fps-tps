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

using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;
using System.Collections.Generic;
using System;
using System.Globalization;
using LiteNetLib.Utils;
using Framework.Game.Server;

namespace Framework.Game.Server
{
    /// <summary>
    /// Core class for server variables
    /// </summary>
    public struct ServerVars : INetSerializable
    {
        /// <summary>
        /// Dictonary which contains server varaibles
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> AllVariables { get; set; }

        /// <summary>
        /// Constructor for server vars
        /// </summary>
        /// <param name="vars">Dictonary which contains server varaibles</param>
        public ServerVars(Dictionary<string, string> vars)
        {
            this.AllVariables = vars;
        }

        /// <summary>
        /// Get an server variable
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string varName, T defaultValue = default(T)) where T : IConvertible
        {
            if (!AllVariables.ContainsKey(varName))
            {
                return defaultValue;
            }
            try
            {
                var value = AllVariables[varName].ToString();
                var formatProvider = CultureInfo.InvariantCulture;
                return (T)Convert.ChangeType(AllVariables[varName].ToString(), typeof(T), formatProvider);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.AllVariables);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.AllVariables = reader.GetDictonaryString();
        }
    }
}