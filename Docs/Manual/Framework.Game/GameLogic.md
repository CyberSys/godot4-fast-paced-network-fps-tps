# GameLogic class

Basic game logic component

```csharp
public class GameLogic : SubViewport, IGameLogic
```

## Public Members

| name | description |
| --- | --- |
| [GameLogic](GameLogic/GameLogic.md)() | The default constructor. |
| [Components](GameLogic/Components.md) { get; } |  |
| [CurrentWorld](GameLogic/CurrentWorld.md) { get; } | The active game world |
| [MapLoader](GameLogic/MapLoader.md) { get; } | The async loader for eg. maps |
| [Services](GameLogic/Services.md) { get; } | Service Registry (Contains all services) |
| virtual [AfterInit](GameLogic/AfterInit.md)() |  |
| virtual [AfterMapDestroy](GameLogic/AfterMapDestroy.md)() |  |
| virtual [AfterMapInstance](GameLogic/AfterMapInstance.md)() |  |
| override [_EnterTree](GameLogic/_EnterTree.md)() | On tree enter |
| override [_ExitTree](GameLogic/_ExitTree.md)() | After exit tree |
| override [_Process](GameLogic/_Process.md)(…) | Update all regisered services |

## Protected Members

| name | description |
| --- | --- |
| [currentWorld](GameLogic/currentWorld.md) |  |
| readonly [secureConnectionKey](GameLogic/secureConnectionKey.md) | Secure passphrase for network connection |
| virtual [LoadWorld](GameLogic/LoadWorld.md)(…) | Load an world by given resource path |
| virtual [OnMapDestroy](GameLogic/OnMapDestroy.md)() | On destroy map |
| virtual [OnMapInstance](GameLogic/OnMapInstance.md)(…) | Instance an map by given PackedScene and WorldTick |

## See Also

* interface [IGameLogic](./IGameLogic.md)
* namespace [Framework.Game](../Framework.md)

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->