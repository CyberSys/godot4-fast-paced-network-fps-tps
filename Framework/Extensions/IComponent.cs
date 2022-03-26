using Godot;

namespace Framework
{

    public interface IBaseComponent
    {
        public void AddChild(Node node, bool legibleUniqueName = false, Node.InternalMode @internal = Node.InternalMode.Disabled);
        public void RemoveChild(Node path);
        public bool IsInsideTree();

        public event Godot.Node.TreeEnteredHandler TreeEntered;

        public ComponentRegistry Components { get; }
    }

    public interface IChildComponent
    {
        public IBaseComponent BaseComponent { get; set; }
    }
}
