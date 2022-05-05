
using Godot;

namespace NetworkingLib
{
#if TOOLS
    [Tool]
    public partial class NetworkingPlugin : EditorPlugin
    {

        public override void _EnterTree()
        {
            //to be continued
            AddCustomType("NetworkServerWorld", "Node3D",
                GD.Load<Script>("res://Framework/Game/Server/NetworkServerWorld.cs"),
                GD.Load<Texture2D>("res://addons/networking/icons/server.png"));

            AddCustomType("NetworkClientWorld", "Node3D",
                GD.Load<Script>("res://Framework/Game/Client/NetworkClientWorld.cs"),
                GD.Load<Texture2D>("res://addons/networking/icons/server.png"));

            AddCustomType("NetworkServerLogic", "SubViewport",
                GD.Load<Script>("res://Framework/Game/Server/NetworkServerLogic.cs"),
                GD.Load<Texture2D>("res://addons/networking/icons/logic.png"));

            AddCustomType("NetworkClientLogic", "SubViewport",
                GD.Load<Script>("res://Framework/Game/Client/NetworkClientLogic.cs"),
                GD.Load<Texture2D>("res://addons/networking/icons/logic.png"));

            AddCustomType("CharacterCamera", "Camera3D",
                GD.Load<Script>("res://Framework/Game/CharacterCamera.cs"),
                GD.Load<Texture2D>("res://addons/networking/icons/logic.png"));
        }

        public override void _ExitTree()
        {
            // Clean-up of the plugin goes here.
        }
    }
#endif
}