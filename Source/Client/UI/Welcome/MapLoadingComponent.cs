using Godot;
using Framework;
using Framework.Game;

namespace Shooter.Client.UI.Welcome
{
    public partial class MapLoadingComponent : CanvasLayer, IChildComponent
    {
        public IBaseComponent BaseComponent { get; set; }

        [Export]
        public NodePath pathToProgressBar { get; set; }

        [Export]
        public NodePath pathToLoadingTextBox { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
            var component = this.BaseComponent as IGameLogic;
            component.MapLoader.OnProgress += (string file, float process) =>
            {
                this.GetNode<Label>(pathToLoadingTextBox).Text = "Loading " + file;
                this.GetNode<ProgressBar>(pathToProgressBar).Value = process;
            };
        }
    }
}
