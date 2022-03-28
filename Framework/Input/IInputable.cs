
using Godot;

namespace Framework.Input
{
    public interface IInputable : IChildComponent
    {
        public IPlayerInput GetPlayerInput();
    }
}