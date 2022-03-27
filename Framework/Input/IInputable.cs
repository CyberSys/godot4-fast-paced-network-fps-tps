
using Godot;

namespace Framework.Input
{
    public interface IInputable : IChildComponent
    {
        public PlayerInputs GetPlayerInput();
    }
}