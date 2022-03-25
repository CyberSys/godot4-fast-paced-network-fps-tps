using Godot;

namespace Shooter.Shared
{
    public interface IComponent<T> where T : Node
    {
        public T MainComponent { get; set; }
    }
}
