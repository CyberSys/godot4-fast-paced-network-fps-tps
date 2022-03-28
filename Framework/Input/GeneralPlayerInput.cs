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
using System.Reflection;
namespace Framework.Input
{
    public struct GeneralPlayerInput : IPlayerInput
    {
        [PlayerInputAttribute]
        public bool Forward, Back, Left, Right, Jump, Fire, Crouch, Shifting;

        public Quaternion ViewDirection { get; set; }

        public bool GetInput(string name)
        {
            var property = this.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                return false;

            var attribute = property.GetCustomAttribute<PlayerInputAttribute>();
            if (attribute == null)
                return false;

            var value = property.GetValue(this);
            if (value == null)
                return false;

            else return (bool)value;
        }

        public float ForwardBackwardAxis
        {
            get
            {
                return Forward ? 1f : Back ? -1f : 0f;
            }
        }

        public float LeftRightAxis
        {
            get
            {
                return Right ? 1f : Left ? -1f : 0f;
            }
        }

        public byte GetKeyBitfield()
        {
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(df => df.GetCustomAttribute<PlayerInputAttribute>() != null).OrderBy(df => df.Name);

            byte bitfield = 0;
            int i = 1;
            foreach (var field in fields)
            {
                var value = (bool)field.GetValueDirect(__makeref(this));
                if (value == true)
                {
                    bitfield |= byte.Parse((i).ToString());
                }

                i *= 2;
            }

            return bitfield;
        }

        public void ApplyKeyBitfield(byte keyFieldData)
        {
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .Where(df => df.GetCustomAttribute<PlayerInputAttribute>() != null).OrderBy(df => df.Name);

            int i = 1;
            foreach (var field in fields)
            {
                var currentByte = byte.Parse((i).ToString());
                var isInUse = (keyFieldData & currentByte) != 0;

                field.SetValueDirect(__makeref(this), isInUse);

                i *= 2;
            }

        }
    }
}