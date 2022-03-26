using Godot;

namespace Framework.Input
{
    public struct PlayerInputs : IPlayerInput
    {
        public bool Forward, Back, Left, Right, Jump, Fire, Crouch, Shifting;

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
            if (Shifting) keyField |= PlayerKeys.Shifting;
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
            Shifting = (keyField & PlayerKeys.Shifting) != 0;
        }
    }
}