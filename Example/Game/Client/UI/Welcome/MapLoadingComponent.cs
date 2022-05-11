using Godot;
using Framework;
using Framework.Game;

namespace Shooter.Client.UI.Welcome
{
    public partial class MapLoadingComponent : CanvasLayer, IChildComponent<GameLogic>
    {

        public GameLogic BaseComponent { get; set; }

        [Export]
        public NodePath pathToProgressBar { get; set; }

        [Export]
        public NodePath pathToLoadingTextBox { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();

            var component = this.BaseComponent as IGameLogic;

            Framework.Utils.AsyncLoader.Loader.OnProgress += this.HandleProcess;
        }

        public void HandleProcess(string file, float process)
        {
            this.GetNode<Label>(pathToLoadingTextBox).Text = "Loading " + file;
            this.GetNode<ProgressBar>(pathToProgressBar).Value = process;
        }

        public override void _ExitTree()
        {
            Framework.Utils.AsyncLoader.Loader.OnProgress -= this.HandleProcess;
            base._ExitTree();
        }
    }
}
