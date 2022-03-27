# Framework assembly

## Framework namespace

| public type | description |
| --- | --- |
| class [ComponentRegistry](./Framework/ComponentRegistry.md) | Component registry class |
| interface [IBaseComponent](./Framework/IBaseComponent.md) |  |
| interface [IChildComponent](./Framework/IChildComponent.md) |  |
| interface [IService](./Framework/IService.md) | Required interface for service classes |
| interface [ITeam](./Framework/ITeam.md) |  |
| static class [Logger](./Framework/Logger.md) |  |
| static class [NetExtensions](./Framework/NetExtensions.md) |  |
| enum [PlayerTeam](./Framework/PlayerTeam.md) |  |
| class [TypeDictonary&lt;T2&gt;](./Framework/TypeDictonary-1.md) | Service registry /service holder of component |

## Framework.Extensions namespace

| public type | description |
| --- | --- |
| class [StringDictonary&lt;T2&gt;](./Framework.Extensions/StringDictonary-1.md) | Service registry /service holder of component |

## Framework.Game namespace

| public type | description |
| --- | --- |
| class [GameLogic](./Framework.Game/GameLogic.md) | Basic game logic component |
| class [GameRule](./Framework.Game/GameRule.md) |  |
| interface [IGameLogic](./Framework.Game/IGameLogic.md) | Required interface for game logic |
| interface [IGameRule](./Framework.Game/IGameRule.md) |  |
| interface [ILevel](./Framework.Game/ILevel.md) | Required interface for levels |
| interface [IPlayer](./Framework.Game/IPlayer.md) | Required interface for players |
| interface [ISimulationAdjuster](./Framework.Game/ISimulationAdjuster.md) |  |
| interface [IWorld](./Framework.Game/IWorld.md) | The required interface for an game world |
| abstract class [Level](./Framework.Game/Level.md) | A basic class for an game level |
| class [NoopAdjuster](./Framework.Game/NoopAdjuster.md) |  |
| abstract class [Player](./Framework.Game/Player.md) | The general player class |
| struct [RegisteredComonent](./Framework.Game/RegisteredComonent.md) |  |
| abstract class [SpawnPoint](./Framework.Game/SpawnPoint.md) | The gernal spawn point class |
| abstract class [World](./Framework.Game/World.md) |  |

## Framework.Game.Client namespace

| public type | description |
| --- | --- |
| abstract class [ClientLogic&lt;T&gt;](./Framework.Game.Client/ClientLogic-1.md) |  |
| abstract class [ClientWorld](./Framework.Game.Client/ClientWorld.md) |  |
| class [LocalPlayer](./Framework.Game.Client/LocalPlayer.md) |  |
| class [PuppetPlayer](./Framework.Game.Client/PuppetPlayer.md) |  |

## Framework.Game.Server namespace

| public type | description |
| --- | --- |
| abstract class [ServerLogic&lt;T&gt;](./Framework.Game.Server/ServerLogic-1.md) |  |
| class [ServerPlayer](./Framework.Game.Server/ServerPlayer.md) |  |
| abstract class [ServerWorld](./Framework.Game.Server/ServerWorld.md) |  |

## Framework.Input namespace

| public type | description |
| --- | --- |
| interface [IInputable](./Framework.Input/IInputable.md) |  |
| interface [IPlayerInput](./Framework.Input/IPlayerInput.md) |  |
| class [PlayerInputProcessor](./Framework.Input/PlayerInputProcessor.md) |  |
| struct [PlayerInputs](./Framework.Input/PlayerInputs.md) |  |
| [Flags] enum [PlayerKeys](./Framework.Input/PlayerKeys.md) |  |
| struct [TickInput](./Framework.Input/TickInput.md) |  |

## Framework.Network namespace

| public type | description |
| --- | --- |
| class [ClientSimulationAdjuster](./Framework.Network/ClientSimulationAdjuster.md) |  |
| abstract class [NetworkPlayer](./Framework.Network/NetworkPlayer.md) |  |
| abstract class [NetworkService](./Framework.Network/NetworkService.md) |  |
| enum [PlayerConnectionState](./Framework.Network/PlayerConnectionState.md) |  |

## Framework.Network.Commands namespace

| public type | description |
| --- | --- |
| struct [ClientInitializer](./Framework.Network.Commands/ClientInitializer.md) |  |
| struct [ClientWorldInitializer](./Framework.Network.Commands/ClientWorldInitializer.md) |  |
| struct [PlayerInputCommand](./Framework.Network.Commands/PlayerInputCommand.md) |  |
| struct [PlayerState](./Framework.Network.Commands/PlayerState.md) |  |
| struct [PlayerUpdate](./Framework.Network.Commands/PlayerUpdate.md) |  |
| struct [ServerInitializer](./Framework.Network.Commands/ServerInitializer.md) |  |
| class [WorldHeartbeat](./Framework.Network.Commands/WorldHeartbeat.md) |  |

## Framework.Network.Services namespace

| public type | description |
| --- | --- |
| struct [ClientConnectionSettings](./Framework.Network.Services/ClientConnectionSettings.md) |  |
| class [ClientNetworkService](./Framework.Network.Services/ClientNetworkService.md) |  |
| class [ServerNetworkService](./Framework.Network.Services/ServerNetworkService.md) |  |

## Framework.Physics namespace

| public type | description |
| --- | --- |
| interface [IMoveable](./Framework.Physics/IMoveable.md) |  |
| interface [IMovementCalculator](./Framework.Physics/IMovementCalculator.md) |  |
| class [InterpolationController](./Framework.Physics/InterpolationController.md) |  |
| class [MovementCalculator](./Framework.Physics/MovementCalculator.md) |  |
| abstract class [PhysicsPlayer](./Framework.Physics/PhysicsPlayer.md) |  |

## Framework.Utils namespace

| public type | description |
| --- | --- |
| class [AsyncLoader](./Framework.Utils/AsyncLoader.md) | Helper class to load resources in background |

<!-- DO NOT EDIT: generated by xmldocmd for Framework.dll -->
