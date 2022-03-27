using Godot;

namespace Framework
{
    /// <summary>
    /// The base component interface required for root components 
    /// </summary>
    public interface IBaseComponent
    {
        /// <summary>
        /// Add an child to the base component
        /// </summary>
        /// <param name="node"></param>
        /// <param name="legibleUniqueName"></param>
        /// <param name="internal"></param>
        public void AddChild(Node node, bool legibleUniqueName = false, Node.InternalMode @internal = Node.InternalMode.Disabled);

        /// <summary>
        /// Delete an child form the base component
        /// </summary>
        /// <param name="path"></param>
        public void RemoveChild(Node path);

        /// <summary>
        /// Check if the component is already inside an tree
        /// </summary>
        /// <returns></returns>
        public bool IsInsideTree();

        /// <summary>
        /// Handle the tree events of godot node tree
        /// </summary>
        public event Godot.Node.TreeEnteredHandler TreeEntered;

        /// <summary>
        /// Contains the component registry class
        /// </summary>
        /// <value></value>
        public ComponentRegistry Components { get; }
    }


    /// <summary>
    /// The child component interface for childs of an base component
    /// </summary>
    public interface IChildComponent
    {
        /// <summary>
        /// The base component of the child component
        /// </summary>
        /// <value></value>
        public IBaseComponent BaseComponent { get; set; }
    }
}
