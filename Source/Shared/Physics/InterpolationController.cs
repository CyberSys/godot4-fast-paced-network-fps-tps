using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Shooter.Shared
{
    public class InterpolationController
    {
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
