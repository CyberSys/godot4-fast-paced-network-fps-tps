# ServerLogic&lt;T&gt; class

```csharp
public abstract class ServerLogic<T> : GameLogic
    where T : ServerWorld
```

## Public Members

| name | description |
| --- | --- |
| [AcceptClients](ServerLogic-1/AcceptClients.md) |  |
| [MaxConnections](ServerLogic-1/MaxConnections.md) |  |
| [NetworkPort](ServerLogic-1/NetworkPort.md) | Network server port |
| virtual [OnPreConnect](ServerLogic-1/OnPreConnect.md)(…) |  |
| virtual [OnServerStarted](ServerLogic-1/OnServerStarted.md)() |  |
| override [_EnterTree](ServerLogic-1/_EnterTree.md)() | Instance the server game logic and load the map from default settings |

## Protected Members

| name | description |
| --- | --- |
| [ServerLogic](ServerLogic-1/ServerLogic.md)() | The default constructor. |
| override [LoadWorld](ServerLogic-1/LoadWorld.md)(…) | Loading the game world (async) |
| override [OnMapDestroy](ServerLogic-1/OnMapDestroy.md)() | Destroy the game world |
| override [OnMapInstance](ServerLogic-1/OnMapInstance.md)(…) | Instancing the game world |

## See Also

* class [GameLogic](../Framework.Game/GameLogic.md)
* class [ServerWorld](./ServerWorld.md)
* namespace [Framework.Game.Server](../Framework.md)

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->