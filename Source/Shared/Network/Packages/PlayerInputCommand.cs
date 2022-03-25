using Godot;
using LiteNetLib.Utils;

namespace Shooter.Shared.Network.Packages
{
    public struct PlayerInputCommand : INetSerializable
    {
        // The world tick for the first input in the array.
        public uint StartWorldTick;

        // An array of inputs, one entry for tick.  Ticks are guaranteed to be contiguous.
        public PlayerInputs[] Inputs;

        // For each input:
        // Delta between the input world tick and the tick the server was at for that input.
        // TODO: This may be overkill, determining an average is probably better, but for now
        // this will give us 100% accuracy over lag compensation.
        public short[] ClientWorldTickDeltas;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.StartWorldTick);

            if (this.Inputs != null)
            {
                writer.Put(this.Inputs.Length);

                for (int i = 0; i < this.Inputs.Length; i++)
                {
                    writer.Put(this.Inputs[i].GetKeyBitfield());
                    writer.Put(this.Inputs[i].ViewDirection);
                    writer.Put(this.ClientWorldTickDeltas[i]);
                }
            }
            else
            {
                writer.Put(0);
            }
        }

        public void Deserialize(NetDataReader reader)
        {

            this.StartWorldTick = reader.GetUInt();
            var length = reader.GetInt();

            this.Inputs = new PlayerInputs[length];
            this.ClientWorldTickDeltas = new short[length];

            for (int i = 0; i < length; i++)
            {
                this.Inputs[i].ApplyKeyBitfield(reader.GetByte());
                this.Inputs[i].ViewDirection = reader.GetQuaternion();
                this.ClientWorldTickDeltas[i] = reader.GetShort();
            }
        }
    }
}