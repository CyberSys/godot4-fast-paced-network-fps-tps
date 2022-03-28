using Framework.Input;
using Framework.Utils;
using Framework.Network.Commands;
using Framework;
using System.Collections.Generic;
using Priority_Queue;

namespace Framework.Input
{
    public struct TickInput
    {
        public uint WorldTick;

        // The remote world tick the player saw other entities at for this input.
        // (This is equivalent to lastServerWorldTick on the client).
        public uint RemoteViewTick;

        public int PlayerId;

        public IPlayerInput Inputs;
    }

    public class PlayerInputProcessor
    {
        private SimplePriorityQueue<TickInput> queue = new SimplePriorityQueue<TickInput>();
        private Dictionary<int, TickInput> latestPlayerInput = new Dictionary<int, TickInput>();

        // Monitoring.
        private MovingAverage averageInputQueueSize = new MovingAverage(10);
        private int staleInputs;

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

        public bool TryGetLatestInput(int playerId, out TickInput ret)
        {
            return latestPlayerInput.TryGetValue(playerId, out ret);
        }

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

        public void EnqueueInput(PlayerInputCommand command, int playerId, uint serverWorldTick)
        {
            // Monitoring.
            // Logger.LogDebug(this, "sv stale inputs => " + staleInputs);

            // Calculate the last tick in the incoming command.
            uint maxTick = command.StartWorldTick + (uint)command.Inputs.Length - 1;

            // Scan for inputs which haven't been handled yet.
            if (maxTick >= serverWorldTick)
            {
                uint start = serverWorldTick > command.StartWorldTick
                    ? serverWorldTick - command.StartWorldTick : 0;
                for (int i = (int)start; i < command.Inputs.Length; ++i)
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
            else
            {
                staleInputs++;
            }
        }
    }
}