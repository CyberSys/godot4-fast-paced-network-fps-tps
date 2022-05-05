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

using Framework.Utils;

namespace Framework.Physics
{
    /// <summary>
    /// Helper class for interpolations by ticks
    /// </summary>
    public class InterpolationController
    {
        /// <summary>
        /// Contains the current interpolation factor
        /// </summary>
        /// <value></value>
        public static float InterpolationFactor { get; private set; } = 1f;

        private DoubleBuffer<float> timestampBuffer = new DoubleBuffer<float>();

        private float totalFixedTime;
        private float totalTime;

        public void ExplicitFixedUpdate(float dt)
        {
            totalFixedTime += dt;
            timestampBuffer.Push(totalFixedTime);
        }

        public void ExplicitUpdate(float dt)
        {
            totalTime += dt;

            float newTime = timestampBuffer.New();
            float oldTime = timestampBuffer.Old();

            if (newTime != oldTime)
            {
                InterpolationFactor = (totalTime - newTime) / (newTime - oldTime);
            }
            else
            {
                InterpolationFactor = 1;
            }
        }
    }
}
