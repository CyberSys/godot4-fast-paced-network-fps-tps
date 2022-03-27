# ServerWorld class

```csharp
public abstract class ServerWorld : World
```

## Public Members

| name | description |
| --- | --- |
| [ActiveGameRule](ServerWorld/ActiveGameRule.md) { get; set; } |  |
| [playerInputProcessor](ServerWorld/playerInputProcessor.md) |  |
| [worldPath](ServerWorld/worldPath.md) |  |
| [AddPlayer&lt;T&gt;](ServerWorld/AddPlayer.md)(…) |  |
| [DeletePlayer](ServerWorld/DeletePlayer.md)(…) |  |
| virtual [OnPlayerConnected](ServerWorld/OnPlayerConnected.md)(…) | Event called after client is connected to server |
| virtual [OnPlayerDisconnect](ServerWorld/OnPlayerDisconnect.md)(…) | Event called after client is disconnected from server |
| [SimulateWorld](ServerWorld/SimulateWorld.md)(…) |  |
| override [Tick](ServerWorld/Tick.md)(…) |  |
| override [_EnterTree](ServerWorld/_EnterTree.md)() | Instancing the server game world |
| override [_Process](ServerWorld/_Process.md)(…) |  |

## Protected Members

| name | description |
| --- | --- |
| [ServerWorld](ServerWorld/ServerWorld.md)() | The default constructor. |

## See Also

* class [World](../Framework.Game/World.md)
* namespace [Framework.Game.Server](../Framework.md)

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->