using System;
using System.Linq;

/// Simple timer which executes a callback at an exactly even rate. For accurate
/// physics simulations.
/// i.e. a "Fixed time step" implementation.
namespace Framework.Utils
{
    public class MovingAverage
    {
        private CircularBuffer<float> buffer;

        public MovingAverage(int windowSize)
        {
            buffer = new CircularBuffer<float>(windowSize);
        }

        public void Push(float value)
        {
            buffer.PushFront(value);
        }

        public float Average()
        {
            return buffer.Sum() / buffer.Count();
        }
    }
}