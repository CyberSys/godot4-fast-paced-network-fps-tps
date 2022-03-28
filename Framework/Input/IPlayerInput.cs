using Godot;
using System;

namespace Framework.Input
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PlayerInputAttribute : Attribute
    {
    }

    public interface IPlayerInput
    {
        public Quaternion ViewDirection { get; set; }
        public bool GetInput(string name);
        public float ForwardBackwardAxis { get; }
        public float LeftRightAxis { get; }

        public byte GetKeyBitfield();
        public void ApplyKeyBitfield(byte keyFieldData);
    }

}