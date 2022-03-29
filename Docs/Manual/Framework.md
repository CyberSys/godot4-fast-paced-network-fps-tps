# Framework assembly

## Framework namespace

| public type | description |
| --- | --- |
| class [ComponentRegistry](./Framework/ComponentRegistry.md) | Component registry class |
| interface [IBaseComponent](./Framework/IBaseComponent.md) | The base component interface required for root components |
| interface [IChildComponent](./Framework/IChildComponent.md) | The child component interface for childs of an base component |
| interface [IService](./Framework/IService.md) | Required interface for service classes |
| interface [ITeam](./Framework/ITeam.md) |  |
| static class [Logger](./Framework/Logger.md) |  |
| static class [NetExtensions](./Framework/NetExtensions.md) |  |
| enum [PlayerTeam](./Framework/PlayerTeam.md) |  |

## Framework.Extensions namespace

| public type | description |
| --- | --- |
| class [StringDictonary&lt;T2&gt;](./Framework.Extensions/StringDictonary-1.md) | String Dictonary contains an string key and an generic object value |
| class [TypeDictonary&lt;T2&gt;](./Framework.Extensions/TypeDictonary-1.md) | TypeDictonary contains an generic type as key and an generic object as value |

## Framework.Game namespace

| public type | description |
| --- | --- |
| struct [AssignedComponent](./Framework.Game/AssignedComponent.md) | An assignable component for remote component injection |
| class [GameLogic](./Framework.Game/GameLogic.md) | Basic game logic component |
| class [GameRule](./Framework.Game/GameRule.md) | Basic class for an game rule |
| interface [IGameLogic](./Framework.Game/IGameLogic.md) | Required interface for game logic |
| interface [IGameRule](./Framework.Game/IGameRule.md) | Required interface for game rules |
| interface [ILevel](./Framework.Game/ILevel.md) | Required interface for levels |
| interface [IPlayer](./Framework.Game/IPlayer.md) | Required interface for players |
| interface [IWorld](./Framework.Game/IWorld.md) | The required interface for an game world |
| abstract class [Level](./Framework.Game/Level.md) | A basic class for an game level |
| abstract class [Player](./Framework.Game/Player.md) | The general player class |
| abstract class [SpawnPoint](./Framework.Game/SpawnPoint.md) | The gernal spawn point class |
| struct [Vars](./Framework.Game/Vars.md) | An dictonary for settings (vars) |
| class [VarsCollection](./Framework.Game/VarsCollection.md) | An collection of vars and config files |
| abstract class [World](./Framework.Game/World.md) | The core world class for server and client worlds |

## Framework.Game.Client namespace

| public type | description |
| --- | --- |
| class [ClientLogic&lt;T&gt;](./Framework.Game.Client/ClientLogic-1.md) | Is the base class for any client (SubViewport) |
| static class [ClientSettings](./Framework.Game.Client/ClientSettings.md) | Static class for client settings |
| abstract class [ClientWorld](./Framework.Game.Client/ClientWorld.md) | Base class for the client world |
| class [LocalPlayer](./Framework.Game.Client/LocalPlayer.md) | The base class for local players |
| class [PuppetPlayer](./Framework.Game.Client/PuppetPlayer.md) | The base class of an puppet player |

## Framework.Game.Server namespace

| public type | description |
| --- | --- |
| abstract class [ServerLogic&lt;T&gt;](./Framework.Game.Server/ServerLogic-1.md) | The core server logic |
| class [ServerPlayer](./Framework.Game.Server/ServerPlayer.md) | Core class for server player |
| abstract class [ServerWorld](./Framework.Game.Server/ServerWorld.md) | Base class for an server world |

## Framework.Input namespace

| public type | description |
| --- | --- |
| struct [GeneralPlayerInput](./Framework.Input/GeneralPlayerInput.md) |  |
| interface [IInputable](./Framework.Input/IInputable.md) |  |
| interface [IPlayerInput](./Framework.Input/IPlayerInput.md) |  |
| class [PlayerInputAttribute](./Framework.Input/PlayerInputAttribute.md) |  |
| class [PlayerInputProcessor](./Framework.Input/PlayerInputProcessor.md) |  |
| struct [TickInput](./Framework.Input/TickInput.md) |  |

## Framework.Network namespace

| public type | description |
| --- | --- |
| class [ClientSimulationAdjuster](./Framework.Network/ClientSimulationAdjuster.md) |  |
| interface [ISimulationAdjuster](./Framework.Network/ISimulationAdjuster.md) | Adjust the server or client tickrate |
| abstract class [NetworkPlayer](./Framework.Network/NetworkPlayer.md) |  |
| abstract class [NetworkService](./Framework.Network/NetworkService.md) |  |
| enum [PlayerConnectionState](./Framework.Network/PlayerConnectionState.md) |  |
| class [ServerSimulationAdjuster](./Framework.Network/ServerSimulationAdjuster.md) | The default simulation adapter for a server instance |

## Framework.Network.Commands namespace

| public type | description |
| --- | --- |
| struct [ClientWorldInitializer](./Framework.Network.Commands/ClientWorldInitializer.md) | Network command for an client, after map was loaded sucessfull Contains all server relevated settings and vars |
| struct [ClientWorldLoader](./Framework.Network.Commands/ClientWorldLoader.md) | Network package for initialize game level and client side |
| struct [PlayerInputCommand](./Framework.Network.Commands/PlayerInputCommand.md) | Network command to send client input to server |
| struct [PlayerState](./Framework.Network.Commands/PlayerState.md) | The player states structures Contains all player realted informations eg. position, rotation, velocity |
| struct [PlayerUpdate](./Framework.Network.Commands/PlayerUpdate.md) |  |
| struct [ServerInitializer](./Framework.Network.Commands/ServerInitializer.md) |  |
| struct [ServerVarUpdate](./Framework.Network.Commands/ServerVarUpdate.md) |  |
| struct [WorldHeartbeat](./Framework.Network.Commands/WorldHeartbeat.md) | The world heartbeat structure for network syncronisation |

## Framework.Network.Services namespace

| public type | description |
| --- | --- |
| struct [ClientConnectionSettings](./Framework.Network.Services/ClientConnectionSettings.md) |  |
| class [ClientNetworkService](./Framework.Network.Services/ClientNetworkService.md) |  |
| class [RconServerService](./Framework.Network.Services/RconServerService.md) |  |
| class [ServerNetworkService](./Framework.Network.Services/ServerNetworkService.md) |  |

## Framework.Physics namespace

| public type | description |
| --- | --- |
| class [DefaultMovementProcessor](./Framework.Physics/DefaultMovementProcessor.md) | An default movement calculator Handles friction, air control, jumping and accelerate |
| interface [IMoveable](./Framework.Physics/IMoveable.md) |  |
| interface [IMovementProcessor](./Framework.Physics/IMovementProcessor.md) | The required interface for movement processors |
| class [InterpolationController](./Framework.Physics/InterpolationController.md) |  |
| abstract class [PhysicsPlayer](./Framework.Physics/PhysicsPlayer.md) | The base class for physics based players (kinematic, rigid..) |

## Framework.Utils namespace

| public type | description |
| --- | --- |
| class [AsyncLoader](./Framework.Utils/AsyncLoader.md) | Helper class to load resources in background |
| class [DoubleBuffer&lt;T&gt;](./Framework.Utils/DoubleBuffer-1.md) |  |
| class [FixedTimer](./Framework.Utils/FixedTimer.md) |  |

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->
