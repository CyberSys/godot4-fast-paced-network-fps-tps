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

using Godot;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System;

namespace Framework
{
    /// <inheritdoc />
    public static class NetExtensions
    {
        private const float QUAT_FLOAT_PRECISION_MULT = 32767f;

        /// <inheritdoc />
        public static T ToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        /// <inheritdoc />
        public static void PutArray<T>(this NetDataWriter writer, T[] array) where T : INetSerializable
        {
            writer.Put((ushort)array.Length);
            foreach (var obj in array)
            {
                writer.Put<T>(obj);
            }
        }

        /// <inheritdoc />
        public static T[] GetArray<T>(this NetDataReader reader) where T : INetSerializable, new()
        {
            var len = reader.GetUShort();
            var array = new T[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = reader.Get<T>();
            }
            return array;
        }

        /// <inheritdoc />
        public static void Put(this NetDataWriter writer, Quaternion quaternion)
        {
            SerializeQuaternion(writer, quaternion);
        }

        /// <inheritdoc />
        public static void Put(this NetDataWriter writer, Vector3 vector)
        {
            SerializeVector3(writer, vector);
        }

        /// <inheritdoc />
        public static void Put(this NetDataWriter writer, Dictionary<string, string> dic)
        {
            SerializeStringDictonary(writer, dic);
        }

        /// <inheritdoc />
        public static Vector3 GetVector3(this NetDataReader reader)
        {
            return DeserializeVector3(reader);
        }

        /// <inheritdoc />
        public static Dictionary<string, string> GetDictonaryString(this NetDataReader reader)
        {
            return DeserializeStringDictonary(reader);
        }

        /// <inheritdoc />
        public static Quaternion GetQuaternion(this NetDataReader reader)
        {
            return DeserializeQuaternion(reader);
        }

        /// <inheritdoc />
        public static void SerializeVector3(NetDataWriter writer, Vector3 vector)
        {
            writer.Put(vector.x);
            writer.Put(vector.y);
            writer.Put(vector.z);
        }

        /// <inheritdoc />
        public static Vector3 DeserializeVector3(NetDataReader reader)
        {
            Vector3 v;
            v.x = reader.GetFloat();
            v.y = reader.GetFloat();
            v.z = reader.GetFloat();
            return v;
        }

        /// <inheritdoc />
        public static void SerializeStringDictonary(NetDataWriter writer, Dictionary<string, string> dic)
        {
            if (dic != null)
            {
                writer.Put(dic.Count);

                foreach (var va in dic)
                {
                    writer.Put(va.Key);
                    writer.Put(va.Value);
                }
            }
            else
            {
                writer.Put(0);
            }
        }

        /// <inheritdoc />
        public static Dictionary<string, string> DeserializeStringDictonary(NetDataReader reader)
        {
            var dictonary = new Dictionary<string, string>();
            var count = reader.GetInt();
            for (int i = 0; i < count; i++)
            {
                dictonary.Add(reader.GetString(), reader.GetString());
            }

            return dictonary;
        }

        /// <inheritdoc />
        public static void SerializeQuaternion(NetDataWriter writer, Quaternion quaternion)
        {
            // Utilize "Smallest three" strategy.
            // Reference: https://gafferongames.com/post/snapshot_compression/
            byte maxIndex = 0;
            float maxValue = float.MinValue;
            float maxValueSign = 1;

            // Find the largest value in the quaternion and save its index.
            for (byte i = 0; i < 4; i++)
            {
                var value = quaternion[i];
                var absValue = Mathf.Abs(value);
                if (absValue > maxValue)
                {
                    maxIndex = i;
                    maxValue = absValue;

                    // Note the sign of the maxValue for later.
                    maxValueSign = Mathf.Sign(value);
                }
            }

            // Encode the smallest three components.
            short a, b, c;
            switch (maxIndex)
            {
                case 0:
                    a = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    b = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    break;
                case 1:
                    a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    b = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    break;
                case 2:
                    a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    b = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    c = (short)(quaternion.w * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    break;
                case 3:
                    a = (short)(quaternion.x * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    b = (short)(quaternion.y * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    c = (short)(quaternion.z * maxValueSign * QUAT_FLOAT_PRECISION_MULT);
                    break;
                default:
                    throw new System.InvalidProgramException("Unexpected quaternion index.");
            }

            writer.Put(maxIndex);
            writer.Put(a);
            writer.Put(b);
            writer.Put(c);
        }

        /// <inheritdoc />
        public static Quaternion DeserializeQuaternion(NetDataReader reader)
        {
            // Read values from the wire and map back to normal float precision.
            byte maxIndex = reader.GetByte();
            float a = reader.GetShort() / QUAT_FLOAT_PRECISION_MULT;
            float b = reader.GetShort() / QUAT_FLOAT_PRECISION_MULT;
            float c = reader.GetShort() / QUAT_FLOAT_PRECISION_MULT;

            // Reconstruct the fourth value.
            float d = Mathf.Sqrt(1f - (a * a + b * b + c * c));
            switch (maxIndex)
            {
                case 0:
                    return new Quaternion(d, a, b, c);
                case 1:
                    return new Quaternion(a, d, b, c);
                case 2:
                    return new Quaternion(a, b, d, c);
                case 3:
                    return new Quaternion(a, b, c, d);
                default:
                    throw new System.InvalidProgramException("Unexpected quaternion index.");
            }
        }
    }
}