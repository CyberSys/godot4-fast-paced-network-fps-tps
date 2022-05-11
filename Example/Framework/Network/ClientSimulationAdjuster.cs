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
    /// <summary>
    /// Helps predict player simulation based on latency. (Only client-sided)
    /// </summary>
    public class ClientSimulationAdjuster : ISimulationAdjuster
    {

        /// <inheritdoc />
        public float AdjustedInterval { get; private set; } = 1.0f;


        /// <inheritdoc />
        private MovingAverage actualTickLeadAvg;

        /// <summary>
        /// Initialize the simulation tick adjuster
        /// </summary>
        /// <param name="serverSendRate">Current server send rate</param>
        public ClientSimulationAdjuster(float serverSendRate)
        {
            this.actualTickLeadAvg = new MovingAverage((int)serverSendRate * 2);
        }

        /// <inheritdoc />
        private int estimatedMissedInputs;

        /// <summary>
        /// Extrapolate based on latency what our client tick should be.
        /// </summary>
        /// <param name="physicsTime"></param>
        /// <param name="receivedServerTick"></param>
        /// <param name="serverLatencyMs"></param>
        /// <returns></returns>
        public uint GuessClientTick(float physicsTime, uint receivedServerTick, int serverLatencyMs)
        {
            float serverLatencySeconds = serverLatencyMs / 1000f;
            uint estimatedTickLead = (uint)(serverLatencySeconds * 1.5 / physicsTime) + 4;
            Logger.LogDebug(this, $"Initializing client with estimated tick lead of {estimatedTickLead}, ping: {serverLatencyMs}");
            return receivedServerTick + estimatedTickLead;
        }

        /// <summary>
        /// Notify actual tick lead
        /// </summary>
        /// <param name="actualTickLead"></param>
        /// <param name="isDebug"></param>
        /// <param name="useLagReduction"></param>
        public void NotifyActualTickLead(int actualTickLead, bool isDebug, bool useLagReduction)
        {
            actualTickLeadAvg.Push(actualTickLead);

            // TODO: This logic needs significant tuning.

            // Negative lead means dropped inputs which is worse than buffering, so immediately move the
            // simulation forward.
            if (actualTickLead < 0)
            {
                //Logger.LogDebug(this, "Dropped an input, got an actual tick lead of " + actualTickLead);
                actualTickLeadAvg.ForceSet(actualTickLead);
                estimatedMissedInputs++;
            }

            var avg = actualTickLeadAvg.Average();
            if (avg <= -16)
            {
                AdjustedInterval = 0.875f;
            }
            else if (avg <= -8)
            {
                AdjustedInterval = 0.9375f;
            }
            else if (avg < 0)
            {
                AdjustedInterval = 0.75f;
            }
            else if (avg < 0)
            {
                AdjustedInterval = 0.96875f;
            }
            else if (avg >= 16)
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

        /// <summary>
        /// Just for debugging
        /// </summary>
        public void Monitoring()
        {
            Logger.SetDebugUI("cl_sim_factor", AdjustedInterval.ToString());
            Logger.SetDebugUI("cl_est_missed_inputs", estimatedMissedInputs.ToString());
        }
    }
}