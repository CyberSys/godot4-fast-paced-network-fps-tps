using System;
using System.Linq;
using Godot;
using Shooter.Client;
using Shooter.Server;
using Framework.Game;
using Framework;
using System.Collections.Generic;

public partial class Bootloader : Node
{
    [Export(PropertyHint.File, "*.tscn")]
    public string ClientLogicScenePath;

    [Export(PropertyHint.File, "*.tscn")]
    public string ServerLogicScenePath;
}
