# Framework assembly

## Framework namespace

| public type | description |
| --- | --- |
| class [ComponentRegistry&lt;T&gt;](./Framework/ComponentRegistry-1.md) | Component registry class |
| interface [IBaseComponent](./Framework/IBaseComponent.md) | The base component interface required for root components |
| interface [IChildComponent&lt;T&gt;](./Framework/IChildComponent-1.md) | The child component interface for childs of an base component |
| interface [IPlayerComponent](./Framework/IPlayerComponent.md) | Interface required for player components |
| static class [IPlayerComponentExtensions](./Framework/IPlayerComponentExtensions.md) |  |
| interface [IService](./Framework/IService.md) | Required interface for service classes |
| static class [Logger](./Framework/Logger.md) | Custom logger of the framework |
| static class [NetExtensions](./Framework/NetExtensions.md) |  |

## Framework.Extensions namespace

| public type | description |
| --- | --- |
| class [StringDictonary&lt;T2&gt;](./Framework.Extensions/StringDictonary-1.md) | String Dictonary contains an string key and an generic object value |
| class [TypeDictonary&lt;T2&gt;](./Framework.Extensions/TypeDictonary-1.md) | TypeDictonary contains an generic type as key and an generic object as value |

## Framework.Game namespace

| public type | description |
| --- | --- |
| enum [CameraMode](./Framework.Game/CameraMode.md) | The camera mode for the player camera |
| class [CharacterCamera](./Framework.Game/CharacterCamera.md) | The player camera for an physics player |
| class [GameLogic](./Framework.Game/GameLogic.md) | Basic game logic component |
| abstract class [GameRule](./Framework.Game/GameRule.md) | Basic class for an game rule |
| interface [IGameLogic](./Framework.Game/IGameLogic.md) | Required interface for game logic |
| interface [IGameRule](./Framework.Game/IGameRule.md) | Required interface for game rules |
| interface [INetworkCharacter](./Framework.Game/INetworkCharacter.md) | Required interface for players |
| interface [INetworkLevel](./Framework.Game/INetworkLevel.md) | Required interface for levels |
| interface [INetworkObject](./Framework.Game/INetworkObject.md) | Required interface for network objects |
| static class [INetworkObjectExtension](./Framework.Game/INetworkObjectExtension.md) |  |
| interface [INetworkWorld](./Framework.Game/INetworkWorld.md) | The required interface for an game world |
| struct [NetworkAttribute](./Framework.Game/NetworkAttribute.md) | Network attribute |
| class [NetworkCharacter](./Framework.Game/NetworkCharacter.md) | The general player class |
| class [NetworkInput](./Framework.Game/NetworkInput.md) | The character or kinematic 3d body node for an network player |
| class [NetworkLevel](./Framework.Game/NetworkLevel.md) | A basic class for an game level |
| enum [NetworkMode](./Framework.Game/NetworkMode.md) | The game object network mode |
| abstract class [NetworkWorld](./Framework.Game/NetworkWorld.md) | The core world class for server and client worlds |
| class [SpawnPoint](./Framework.Game/SpawnPoint.md) | The gernal spawn point class |
| struct [Vars](./Framework.Game/Vars.md) | An dictonary for settings (vars) |
| class [VarsCollection](./Framework.Game/VarsCollection.md) | An collection of vars and config files |

## Framework.Game.Client namespace

| public type | description |
| --- | --- |
| static class [ClientSettings](./Framework.Game.Client/ClientSettings.md) | Static class for client settings |
| static class [MiscExtensions](./Framework.Game.Client/MiscExtensions.md) |  |
| class [NetworkClientLogic](./Framework.Game.Client/NetworkClientLogic.md) | Is the base class for any client (SubViewport) |
| class [NetworkClientWorld](./Framework.Game.Client/NetworkClientWorld.md) | Base class for the client world |

## Framework.Game.Server namespace

| public type | description |
| --- | --- |
| class [NetworkServerLogic](./Framework.Game.Server/NetworkServerLogic.md) | The core server logic |
| class [NetworkServerWorld](./Framework.Game.Server/NetworkServerWorld.md) | Base class for an server world |

## Framework.Input namespace

| public type | description |
| --- | --- |
| struct [GeneralPlayerInput](./Framework.Input/GeneralPlayerInput.md) | Default class for player input |
| interface [IInputProcessor](./Framework.Input/IInputProcessor.md) | Required interface for local players with input eg. shifting, crouching, moving, etc. |
| interface [IPlayerInput](./Framework.Input/IPlayerInput.md) | The network package required for sending inputs |
| struct [PlayerInput](./Framework.Input/PlayerInput.md) | Network command to send client input to server |
| class [PlayerInputProcessor](./Framework.Input/PlayerInputProcessor.md) | Processing inputs from an given queue |
| struct [TickInput](./Framework.Input/TickInput.md) | The input for an given world tick of on specfic player (server-sided) |

## Framework.Network namespace

| public type | description |
| --- | --- |
| struct [ClientConnectionSettings](./Framework.Network/ClientConnectionSettings.md) | Settings for the client connection |
| class [ClientNetworkService](./Framework.Network/ClientNetworkService.md) | The client network service |
| class [ClientSimulationAdjuster](./Framework.Network/ClientSimulationAdjuster.md) | Helps predict player simulation based on latency. (Only client-sided) |
| interface [ISimulationAdjuster](./Framework.Network/ISimulationAdjuster.md) | Adjust the server or client tickrate |
| abstract class [NetworkService](./Framework.Network/NetworkService.md) | Base network service class |
| [Flags] enum [NetworkSyncFrom](./Framework.Network/NetworkSyncFrom.md) |  |
| [Flags] enum [NetworkSyncTo](./Framework.Network/NetworkSyncTo.md) |  |
| class [NetworkVar](./Framework.Network/NetworkVar.md) |  |
| enum [PlayerConnectionState](./Framework.Network/PlayerConnectionState.md) | Connection states of the player |
| class [RconServerService](./Framework.Network/RconServerService.md) | The rcon network service |
| class [ServerNetworkService](./Framework.Network/ServerNetworkService.md) | The server network service |
| class [ServerSimulationAdjuster](./Framework.Network/ServerSimulationAdjuster.md) | The default simulation adapter for a server instance |

## Framework.Network.Commands namespace

| public type | description |
| --- | --- |
| class [ClientWorldInitializer](./Framework.Network.Commands/ClientWorldInitializer.md) | Network command for an client, after map was loaded sucessfull Contains all server relevated settings and vars |
| struct [ClientWorldLoader](./Framework.Network.Commands/ClientWorldLoader.md) | Network package for initialize game level and client side |
| class [PlayerCollection](./Framework.Network.Commands/PlayerCollection.md) | Network package for notification of new players or players changes over all players. |
| struct [PlayerInfo](./Framework.Network.Commands/PlayerInfo.md) | Network package for notification of new players or players changes. |
| class [PlayerLeave](./Framework.Network.Commands/PlayerLeave.md) | Network package for notification when player leaves the game. |
| struct [PlayerNetworkVarState](./Framework.Network.Commands/PlayerNetworkVarState.md) |  |
| struct [PlayerState](./Framework.Network.Commands/PlayerState.md) | The player states structures Contains all player realted informations eg. position, rotation, velocity |
| enum [PlayerStateVarType](./Framework.Network.Commands/PlayerStateVarType.md) |  |
| struct [ServerInitializer](./Framework.Network.Commands/ServerInitializer.md) | Network command to tell the server that the client world is initalized |
| class [ServerVarUpdate](./Framework.Network.Commands/ServerVarUpdate.md) | Network command for updaing server vars on client side |
| class [WorldHeartbeat](./Framework.Network.Commands/WorldHeartbeat.md) | The world heartbeat structure for network syncronisation |

## Framework.Physics namespace

| public type | description |
| --- | --- |
| class [DefaultMovementProcessor](./Framework.Physics/DefaultMovementProcessor.md) | An default movement calculator Handles friction, air control, jumping and accelerate |
| class [HitBox](./Framework.Physics/HitBox.md) | Hit box collider |
| interface [IMovementProcessor](./Framework.Physics/IMovementProcessor.md) | The required interface for movement processors |
| class [InterpolationController](./Framework.Physics/InterpolationController.md) | Helper class for interpolations by ticks |
| class [RayCastHit](./Framework.Physics/RayCastHit.md) | The hit result of an ray cast |
| struct [RaycastTest](./Framework.Physics/RaycastTest.md) | Network command for debugging raycasts between server nd client |

## Framework.Utils namespace

| public type | description |
| --- | --- |
| class [AsyncLoader](./Framework.Utils/AsyncLoader.md) | Helper class to load resources in background |
| class [DoubleBuffer&lt;T&gt;](./Framework.Utils/DoubleBuffer-1.md) |  |
| class [FixedTimer](./Framework.Utils/FixedTimer.md) |  |
| class [LineDrawer3d](./Framework.Utils/LineDrawer3d.md) |  |
| class [RayCastLine](./Framework.Utils/RayCastLine.md) |  |
| static class [VectorExtension](./Framework.Utils/VectorExtension.md) |  |

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->
