using Godot;
using Shooter.Shared;
using System;
using LiteNetLib.Utils;

namespace Shooter.Shared
{
    [Flags]
    public enum PlayerKeys : byte
    {
        None = 0,
        Forward = 1,
        Back = 2,
        Right = 4,
        Left = 8,
        Jump = 16,
        Fire = 32,
        Crouch = 64,
    }

    public struct PlayerInputs
    {
        public bool Forward, Back, Left, Right, Jump, Fire, Crouch;

        public Quaternion ViewDirection;

        public float ForwardAxis
        {
            get
            {
                return Forward ? 1f : Back ? -1f : 0f;
            }
        }

        public float RightAxis
        {
            get
            {
                return Right ? 1f : Left ? -1f : 0f;
            }
        }

        public byte GetKeyBitfield()
        {
            var keyField = PlayerKeys.None;
            if (Forward) keyField |= PlayerKeys.Forward;
            if (Back) keyField |= PlayerKeys.Back;
            if (Right) keyField |= PlayerKeys.Right;
            if (Left) keyField |= PlayerKeys.Left;
            if (Jump) keyField |= PlayerKeys.Jump;
            if (Fire) keyField |= PlayerKeys.Fire;
            if (Crouch) keyField |= PlayerKeys.Crouch;
            return (byte)keyField;
        }

        public void ApplyKeyBitfield(byte keyFieldData)
        {
            var keyField = (PlayerKeys)keyFieldData;
            Forward = (keyField & PlayerKeys.Forward) != 0;
            Back = (keyField & PlayerKeys.Back) != 0;
            Right = (keyField & PlayerKeys.Right) != 0;
            Left = (keyField & PlayerKeys.Left) != 0;
            Jump = (keyField & PlayerKeys.Jump) != 0;
            Fire = (keyField & PlayerKeys.Fire) != 0;
            Crouch = (keyField & PlayerKeys.Crouch) != 0;
        }
    }
}