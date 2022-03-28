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

using System.Diagnostics;
using Framework;
using Framework.Utils;
using Framework.Game;

namespace Framework.Network
{
    public class ClientSimulationAdjuster : ISimulationAdjuster
    {

        /// <inheritdoc />
        public float AdjustedInterval { get; private set; } = 1.0f;


        /// <inheritdoc />
        private MovingAverage actualTickLeadAvg;

        public ClientSimulationAdjuster(float serverSendRate)
        {
            this.actualTickLeadAvg = new MovingAverage((int)serverSendRate * 2);
        }

        /// <inheritdoc />
        private int estimatedMissedInputs;

        /// <inheritdoc />
        private Stopwatch droppedInputTimer = new Stopwatch();

        // Extrapolate based on latency what our client tick should be.
        public uint GuessClientTick(float physicsTime, uint receivedServerTick, int serverLatencyMs)
        {
            float serverLatencySeconds = serverLatencyMs / 1000f;
            uint estimatedTickLead = (uint)(serverLatencySeconds * 1.5 / physicsTime) + 4;
            Logger.LogDebug(this, $"Initializing client with estimated tick lead of {estimatedTickLead}, ping: {serverLatencyMs}");
            return receivedServerTick + estimatedTickLead;
        }

        public void NotifyActualTickLead(int actualTickLead, bool isDebug, bool useLagReduction)
        {
            actualTickLeadAvg.Push(actualTickLead);

            // TODO: This logic needs significant tuning.

            // Negative lead means dropped inputs which is worse than buffering, so immediately move the
            // simulation forward.
            if (actualTickLead < 0)
            {
                //Logger.LogDebug(this, "Dropped an input, got an actual tick lead of " + actualTickLead);
                droppedInputTimer.Restart();
                estimatedMissedInputs++;
            }

            var avg = actualTickLeadAvg.Average();
            if (droppedInputTimer.IsRunning && droppedInputTimer.ElapsedMilliseconds < 1000 || isDebug)
            {
                if (avg <= -16)
                {
                    AdjustedInterval = 0.875f;
                }
                else if (avg <= -8)
                {
                    AdjustedInterval = 0.9375f;
                }
                else
                {
                    AdjustedInterval = 0.96875f;
                }

                return;
            }

            // Check for a steady average of a healthy connection before backing off the simulation.
            if (avg >= 16)
            {
                AdjustedInterval = 1.125f;
            }
            else if (avg >= 8)
            {
                AdjustedInterval = 1.0625f;
            }
            else if (avg >= 4)
            {
                AdjustedInterval = 1.03125f;
            }
            else if (avg >= 2 && useLagReduction)
            {
                AdjustedInterval = 1.015625f;
            }
            else
            {
                AdjustedInterval = 1f;
            }
        }

        public void Monitoring()
        {
            Logger.SetDebugUI("cl sim factor", AdjustedInterval.ToString());
            Logger.SetDebugUI("cl est. missed inputs", estimatedMissedInputs.ToString());
        }
    }
}