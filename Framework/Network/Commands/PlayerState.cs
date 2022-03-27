using Godot;
using LiteNetLib.Utils;

namespace Framework.Network.Commands
{
    public struct PlayerState : INetSerializable
    {
        public int Id;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public bool Grounded;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(this.Id);

            writer.Put(this.Position);
            writer.Put(this.Rotation);
            writer.Put(this.Velocity);

            writer.Put(this.Grounded);
        }

        public void Deserialize(NetDataReader reader)
        {
            this.Id = reader.GetInt();
            this.Position = reader.GetVector3();
            this.Rotation = reader.GetQuaternion();
            this.Velocity = reader.GetVector3();
            this.Grounded = reader.GetBool();
        }
    }
}
