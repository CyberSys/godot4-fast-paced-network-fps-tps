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
        /// The byte which contains all inputs
        /// </summary>
        /// <value></value>
        public byte Input { get; set; }

        /// <summary>
        /// The view direction or camera direction
        /// </summary>
        /// <value></value>
        public Vector3 ViewDirection { get; set; }

        /// <summary>
        /// The current activated input keys
        /// </summary>
        /// <value></value>
        public string[] CurrentInput { get; private set; }

        /// <summary>
        /// Get input by given string, related to booleans with PlayerInputAttribute
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetInput(string name)
        {
            if (this.CurrentInput == null || this.CurrentInput.Length <= 0)
            {
                return false;
            }

            return this.CurrentInput.Contains(name);
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.Input);
            writer.Put(this.ViewDirection);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            this.Input = reader.GetByte();
            this.ViewDirection = reader.GetVector3();
        }

        /// <summary>
        /// Deserialize input byte with an given list of input keys
        /// </summary>
        /// <param name="InputKeys"></param>
        /// <returns></returns>
        public GeneralPlayerInput DeserliazeWithInputKeys(List<string> InputKeys)
        {
            if (InputKeys == null)
                return this;

            int i = 1;
            var listOfkeys = new List<string>();
            foreach (var key in InputKeys.OrderBy(df => df))
            {
                var currentByte = byte.Parse((i).ToString());
                var isInUse = (this.Input & currentByte) != 0;
                if (isInUse)
                {
                    listOfkeys.Add(key);
                }
                i *= 2;
            }

            this.CurrentInput = listOfkeys.ToArray();
            return this;
        }

        /// <summary>
        /// Apply input keys with an existing list of avaible keys.
        /// Creating an bit mask
        /// </summary>
        /// <param name="InputKeys"></param>
        /// <param name="activeKeys"></param>
        public void Apply(List<string> InputKeys, Dictionary<string, bool> activeKeys)
        {
            if (InputKeys == null)
                return;

            if (activeKeys == null)
                return;

            int i = 1;
            byte input = 0;
            var listOfActiveKeys = new List<string>();
            foreach (var key in InputKeys.OrderBy(df => df))
            {
                if (activeKeys.ContainsKey(key) && activeKeys[key] == true)
                {
                    input |= byte.Parse((i).ToString());
                    listOfActiveKeys.Add(key);
                }

                i *= 2;
            }

            this.CurrentInput = listOfActiveKeys.ToArray();
            this.Input = input;
        }
    }
}