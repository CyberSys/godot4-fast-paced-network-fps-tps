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

using System.Linq;
using System;
using Godot;
using LiteNetLib.Utils;
using System.Reflection;
using System.Collections.Generic;
namespace Framework.Input
{
    /// <summary>
    /// Default class for player input
    /// </summary>
    public struct GeneralPlayerInput : IPlayerInput
    {
        /// <summary>
        /// Keys which enabled
        /// </summary>
        /// <value></value>
        public List<string> InputKeys { get; private set; }

        /// <summary>
        /// The view direction or camera direction
        /// </summary>
        /// <value></value>
        public Quaternion ViewDirection { get; set; }

        /// <summary>
        /// Enable an boolean (input) for given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetInput(string key, bool value = true)
        {
            if (value == true)
            {
                if (this.InputKeys == null)
                {
                    this.InputKeys = new List<string>();
                }

                this.InputKeys.Add(key);
            }
        }

        /// <summary>
        /// Get input by given string, related to booleans with PlayerInputAttribute
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetInput(string name)
        {
            if (this.InputKeys == null)
            {
                this.InputKeys = new List<string>();
            }

            return this.InputKeys.Contains(name);
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            if (this.InputKeys == null)
            {
                this.InputKeys = new List<string>();
            }

            writer.PutArray(this.InputKeys.ToArray());
            writer.Put(this.ViewDirection);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.InputKeys = reader.GetStringArray().ToList<string>();
            if (this.InputKeys == null)
            {
                this.InputKeys = new List<string>();
            }

            this.ViewDirection = reader.GetQuaternion();
        }
    }
}