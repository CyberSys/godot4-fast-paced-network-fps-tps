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

using Framework.Input;
using Framework.Utils;
using Framework.Network.Commands;
using Framework;
using System.Collections.Generic;
using Priority_Queue;

namespace Framework.Input
{
    /// <summary>
    /// Processing inputs from an given queue
    /// </summary>
    public class PlayerInputProcessor
    {
        private SimplePriorityQueue<TickInput> queue = new SimplePriorityQueue<TickInput>();
        private Dictionary<short, TickInput> latestPlayerInput = new Dictionary<short, TickInput>();

        // Monitoring.
        private MovingAverage averageInputQueueSize = new MovingAverage(10);
        private int staleInputs;

        /// <summary>
        /// Log stats for queue input
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="worldTick"></param>
        public void LogQueueStatsForPlayer(int playerId, uint worldTick)
        {
            int count = 0;
            foreach (var entry in queue)
            {
                if (entry.PlayerId == playerId && entry.WorldTick >= worldTick)
                {
                    count++;
                    worldTick++;
                }
            }
            averageInputQueueSize.Push(count);
            Logger.SetDebugUI("sv_avg_input_queue", averageInputQueueSize.ToString());
        }

        /// <summary>
        /// Get an collection of the last inputs since an given tick and dequeue them
        /// </summary>
        /// <param name="worldTick"></param>
        /// <returns></returns>
        public List<TickInput> DequeueInputsForTick(uint worldTick)
        {
            var ret = new List<TickInput>();
            TickInput entry;
            while (queue.TryDequeue(out entry))
            {
                if (entry.WorldTick < worldTick)
                {
                }
                else if (entry.WorldTick == worldTick)
                {
                    ret.Add(entry);
                }
                else
                {
                    // We dequeued a future input, put it back in.
                    queue.Enqueue(entry, entry.WorldTick);
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Get the last player input tick
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public uint GetLatestPlayerInputTick(short playerId)
        {
            TickInput input;
            if (!TryGetLatestInput(playerId, out input))
            {
                return 0;
            }
            return input.WorldTick;
        }

        /// <summary>
        /// Try to get the last input
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetLatestInput(short playerId, out TickInput ret)
        {
            return latestPlayerInput.TryGetValue(playerId, out ret);
        }

        /// <summary>
        /// Add new input to input queue
        /// </summary>
        /// <param name="command"></param>
        /// <param name="playerId"></param>
        /// <param name="lastAckedInputTick"></param>
        public void EnqueueInput(PlayerInput command, short playerId, uint lastAckedInputTick)
        {
            // Monitoring.
            // Logger.LogDebug(this, "sv stale inputs => " + staleInputs);

            // Calculate the last tick in the incoming command.
            uint maxTick = command.StartWorldTick + (uint)command.Inputs.Length - 1;
            uint startIndex = lastAckedInputTick >= command.StartWorldTick
               ? lastAckedInputTick - command.StartWorldTick + 1
               : 0;

            // Scan for inputs which haven't been handled yet.
            for (int i = (int)startIndex; i < command.Inputs.Length; ++i)
            {
                // Apply inputs to the associated player controller and simulate the world.
                var worldTick = command.StartWorldTick + i;
                var tickInput = new TickInput
                {
                    WorldTick = (uint)worldTick,
                    RemoteViewTick = (uint)(worldTick - command.ClientWorldTickDeltas[i]),
                    PlayerId = playerId,
                    Inputs = command.Inputs[i],
                };
                queue.Enqueue(tickInput, worldTick);

                // Store the latest input in case the simulation needs to repeat missed frames.
                latestPlayerInput[playerId] = tickInput;
            }
        }
    }
}