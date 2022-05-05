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

using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Framework.Game
{

    /// <summary>
    /// An dictonary for settings (vars)
    /// </summary>
    public struct Vars : INetSerializable
    {
        /// <summary>
        /// Dictonary which contains server varaibles
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> AllVariables { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Constructor for server vars
        /// </summary>
        /// <param name="vars">Dictonary which contains server varaibles</param>
        public Vars(Dictionary<string, string> vars)
        {
            this.AllVariables = vars;
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