<a name='assembly'></a>
# Framework

## Contents

- [AsyncLoader](#T-Framework-Utils-AsyncLoader 'Framework.Utils.AsyncLoader')
  - [Tick()](#M-Framework-Utils-AsyncLoader-Tick 'Framework.Utils.AsyncLoader.Tick')
- [CircularBuffer\`1](#T-Framework-Utils-CircularBuffer`1 'Framework.Utils.CircularBuffer`1')
  - [#ctor(capacity,items)](#M-Framework-Utils-CircularBuffer`1-#ctor-System-Int32,`0[]- 'Framework.Utils.CircularBuffer`1.#ctor(System.Int32,`0[])')
  - [_end](#F-Framework-Utils-CircularBuffer`1-_end 'Framework.Utils.CircularBuffer`1._end')
  - [_size](#F-Framework-Utils-CircularBuffer`1-_size 'Framework.Utils.CircularBuffer`1._size')
  - [_start](#F-Framework-Utils-CircularBuffer`1-_start 'Framework.Utils.CircularBuffer`1._start')
  - [Capacity](#P-Framework-Utils-CircularBuffer`1-Capacity 'Framework.Utils.CircularBuffer`1.Capacity')
  - [Size](#P-Framework-Utils-CircularBuffer`1-Size 'Framework.Utils.CircularBuffer`1.Size')
  - [Back()](#M-Framework-Utils-CircularBuffer`1-Back 'Framework.Utils.CircularBuffer`1.Back')
  - [Decrement(index)](#M-Framework-Utils-CircularBuffer`1-Decrement-System-Int32@- 'Framework.Utils.CircularBuffer`1.Decrement(System.Int32@)')
  - [Front()](#M-Framework-Utils-CircularBuffer`1-Front 'Framework.Utils.CircularBuffer`1.Front')
  - [Increment(index)](#M-Framework-Utils-CircularBuffer`1-Increment-System-Int32@- 'Framework.Utils.CircularBuffer`1.Increment(System.Int32@)')
  - [InternalIndex(index)](#M-Framework-Utils-CircularBuffer`1-InternalIndex-System-Int32- 'Framework.Utils.CircularBuffer`1.InternalIndex(System.Int32)')
  - [PopBack()](#M-Framework-Utils-CircularBuffer`1-PopBack 'Framework.Utils.CircularBuffer`1.PopBack')
  - [PopFront()](#M-Framework-Utils-CircularBuffer`1-PopFront 'Framework.Utils.CircularBuffer`1.PopFront')
  - [PushBack(item)](#M-Framework-Utils-CircularBuffer`1-PushBack-`0- 'Framework.Utils.CircularBuffer`1.PushBack(`0)')
  - [PushFront(item)](#M-Framework-Utils-CircularBuffer`1-PushFront-`0- 'Framework.Utils.CircularBuffer`1.PushFront(`0)')
  - [ToArray()](#M-Framework-Utils-CircularBuffer`1-ToArray 'Framework.Utils.CircularBuffer`1.ToArray')
- [ClientWorld](#T-Framework-Game-Client-ClientWorld 'Framework.Game.Client.ClientWorld')
  - [clientSimulationAdjuster](#F-Framework-Game-Client-ClientWorld-clientSimulationAdjuster 'Framework.Game.Client.ClientWorld.clientSimulationAdjuster')
  - [lastServerWorldTick](#F-Framework-Game-Client-ClientWorld-lastServerWorldTick 'Framework.Game.Client.ClientWorld.lastServerWorldTick')
  - [localPlayer](#F-Framework-Game-Client-ClientWorld-localPlayer 'Framework.Game.Client.ClientWorld.localPlayer')
  - [replayedStates](#F-Framework-Game-Client-ClientWorld-replayedStates 'Framework.Game.Client.ClientWorld.replayedStates')
  - [worldStateQueue](#F-Framework-Game-Client-ClientWorld-worldStateQueue 'Framework.Game.Client.ClientWorld.worldStateQueue')
  - [ExecuteHeartbeat(update)](#M-Framework-Game-Client-ClientWorld-ExecuteHeartbeat-Framework-Network-Commands-WorldHeartbeat- 'Framework.Game.Client.ClientWorld.ExecuteHeartbeat(Framework.Network.Commands.WorldHeartbeat)')
  - [PostUpdate(interval)](#M-Framework-Game-Client-ClientWorld-PostUpdate 'Framework.Game.Client.ClientWorld.PostUpdate')
  - [ProcessServerWorldState(incomingState)](#M-Framework-Game-Client-ClientWorld-ProcessServerWorldState-Framework-Network-Commands-WorldHeartbeat- 'Framework.Game.Client.ClientWorld.ProcessServerWorldState(Framework.Network.Commands.WorldHeartbeat)')
  - [Tick(interval)](#M-Framework-Game-Client-ClientWorld-Tick-System-Single- 'Framework.Game.Client.ClientWorld.Tick(System.Single)')
- [ComponentRegistry](#T-Framework-ComponentRegistry 'Framework.ComponentRegistry')
  - [#ctor(baseComponent)](#M-Framework-ComponentRegistry-#ctor-Framework-IBaseComponent- 'Framework.ComponentRegistry.#ctor(Framework.IBaseComponent)')
  - [All](#P-Framework-ComponentRegistry-All 'Framework.ComponentRegistry.All')
  - [AddComponentAsync\`\`1(resourcePath,callback)](#M-Framework-ComponentRegistry-AddComponentAsync``1-System-String,System-Action{System-Boolean}- 'Framework.ComponentRegistry.AddComponentAsync``1(System.String,System.Action{System.Boolean})')
  - [AddComponent\`\`1()](#M-Framework-ComponentRegistry-AddComponent``1 'Framework.ComponentRegistry.AddComponent``1')
  - [AddComponent\`\`1(resourcePath)](#M-Framework-ComponentRegistry-AddComponent``1-System-String- 'Framework.ComponentRegistry.AddComponent``1(System.String)')
  - [DeleteComponent\`\`1()](#M-Framework-ComponentRegistry-DeleteComponent``1 'Framework.ComponentRegistry.DeleteComponent``1')
  - [Get\`\`1()](#M-Framework-ComponentRegistry-Get``1 'Framework.ComponentRegistry.Get``1')
  - [TryGetService()](#M-Framework-ComponentRegistry-TryGetService-System-Type,Godot-Node@- 'Framework.ComponentRegistry.TryGetService(System.Type,Godot.Node@)')
- [GameLogic](#T-Framework-Game-GameLogic 'Framework.Game.GameLogic')
  - [#ctor()](#M-Framework-Game-GameLogic-#ctor 'Framework.Game.GameLogic.#ctor')
  - [_serviceRegistry](#F-Framework-Game-GameLogic-_serviceRegistry 'Framework.Game.GameLogic._serviceRegistry')
  - [currentWorld](#F-Framework-Game-GameLogic-currentWorld 'Framework.Game.GameLogic.currentWorld')
  - [loader](#F-Framework-Game-GameLogic-loader 'Framework.Game.GameLogic.loader')
  - [mapLoadInProcess](#F-Framework-Game-GameLogic-mapLoadInProcess 'Framework.Game.GameLogic.mapLoadInProcess')
  - [secureConnectionKey](#F-Framework-Game-GameLogic-secureConnectionKey 'Framework.Game.GameLogic.secureConnectionKey')
  - [CurrentWorld](#P-Framework-Game-GameLogic-CurrentWorld 'Framework.Game.GameLogic.CurrentWorld')
  - [MapLoader](#P-Framework-Game-GameLogic-MapLoader 'Framework.Game.GameLogic.MapLoader')
  - [Services](#P-Framework-Game-GameLogic-Services 'Framework.Game.GameLogic.Services')
  - [LoadWorld(path,worldTick)](#M-Framework-Game-GameLogic-LoadWorld-System-String,System-UInt32- 'Framework.Game.GameLogic.LoadWorld(System.String,System.UInt32)')
  - [OnMapDestroy()](#M-Framework-Game-GameLogic-OnMapDestroy 'Framework.Game.GameLogic.OnMapDestroy')
  - [OnMapInstance()](#M-Framework-Game-GameLogic-OnMapInstance-Godot-PackedScene,System-UInt32- 'Framework.Game.GameLogic.OnMapInstance(Godot.PackedScene,System.UInt32)')
  - [_EnterTree()](#M-Framework-Game-GameLogic-_EnterTree 'Framework.Game.GameLogic._EnterTree')
  - [_ExitTree()](#M-Framework-Game-GameLogic-_ExitTree 'Framework.Game.GameLogic._ExitTree')
  - [_Process(delta)](#M-Framework-Game-GameLogic-_Process-System-Single- 'Framework.Game.GameLogic._Process(System.Single)')
- [IGameLogic](#T-Framework-Game-IGameLogic 'Framework.Game.IGameLogic')
  - [Services](#P-Framework-Game-IGameLogic-Services 'Framework.Game.IGameLogic.Services')
- [ILevel](#T-Framework-Game-ILevel 'Framework.Game.ILevel')
  - [GetAllSpawnPoints()](#M-Framework-Game-ILevel-GetAllSpawnPoints 'Framework.Game.ILevel.GetAllSpawnPoints')
- [IPlayer](#T-Framework-Game-IPlayer 'Framework.Game.IPlayer')
  - [Id](#P-Framework-Game-IPlayer-Id 'Framework.Game.IPlayer.Id')
  - [Latency](#P-Framework-Game-IPlayer-Latency 'Framework.Game.IPlayer.Latency')
  - [PlayerName](#P-Framework-Game-IPlayer-PlayerName 'Framework.Game.IPlayer.PlayerName')
  - [State](#P-Framework-Game-IPlayer-State 'Framework.Game.IPlayer.State')
  - [Team](#P-Framework-Game-IPlayer-Team 'Framework.Game.IPlayer.Team')
- [IService](#T-Framework-IService 'Framework.IService')
  - [Register()](#M-Framework-IService-Register 'Framework.IService.Register')
  - [Render(delta)](#M-Framework-IService-Render-System-Single- 'Framework.IService.Render(System.Single)')
  - [Unregister()](#M-Framework-IService-Unregister 'Framework.IService.Unregister')
  - [Update(delta)](#M-Framework-IService-Update-System-Single- 'Framework.IService.Update(System.Single)')
- [IWorld](#T-Framework-Game-IWorld 'Framework.Game.IWorld')
  - [Level](#P-Framework-Game-IWorld-Level 'Framework.Game.IWorld.Level')
  - [Players](#P-Framework-Game-IWorld-Players 'Framework.Game.IWorld.Players')
  - [ServerVars](#P-Framework-Game-IWorld-ServerVars 'Framework.Game.IWorld.ServerVars')
  - [WorldTick](#P-Framework-Game-IWorld-WorldTick 'Framework.Game.IWorld.WorldTick')
  - [Destroy()](#M-Framework-Game-IWorld-Destroy 'Framework.Game.IWorld.Destroy')
  - [Init(serverVars,initalWorldTick)](#M-Framework-Game-IWorld-Init-System-Collections-Generic-Dictionary{System-String,System-String},System-UInt32- 'Framework.Game.IWorld.Init(System.Collections.Generic.Dictionary{System.String,System.String},System.UInt32)')
- [Level](#T-Framework-Game-Level 'Framework.Game.Level')
  - [GetAllSpawnPoints()](#M-Framework-Game-Level-GetAllSpawnPoints 'Framework.Game.Level.GetAllSpawnPoints')
  - [GetFreeSpawnPoints()](#M-Framework-Game-Level-GetFreeSpawnPoints 'Framework.Game.Level.GetFreeSpawnPoints')
- [MovementCalculator](#T-Framework-Physics-MovementCalculator 'Framework.Physics.MovementCalculator')
  - [AirControl()](#M-Framework-Physics-MovementCalculator-AirControl-Godot-Vector3,System-Single,System-Single- 'Framework.Physics.MovementCalculator.AirControl(Godot.Vector3,System.Single,System.Single)')
  - [AirMove()](#M-Framework-Physics-MovementCalculator-AirMove-System-Single- 'Framework.Physics.MovementCalculator.AirMove(System.Single)')
  - [ApplyFriction()](#M-Framework-Physics-MovementCalculator-ApplyFriction-System-Single,System-Single- 'Framework.Physics.MovementCalculator.ApplyFriction(System.Single,System.Single)')
- [NetworkService](#T-Framework-Network-NetworkService 'Framework.Network.NetworkService')
  - [_netPacketProcessor](#F-Framework-Network-NetworkService-_netPacketProcessor 'Framework.Network.NetworkService._netPacketProcessor')
  - [SendMessageSerialisable\`\`1(peerId,command,method)](#M-Framework-Network-NetworkService-SendMessageSerialisable``1-System-Int32,``0,LiteNetLib-DeliveryMethod- 'Framework.Network.NetworkService.SendMessageSerialisable``1(System.Int32,``0,LiteNetLib.DeliveryMethod)')
  - [SendMessage\`\`1(peerId,obj,method)](#M-Framework-Network-NetworkService-SendMessage``1-System-Int32,``0,LiteNetLib-DeliveryMethod- 'Framework.Network.NetworkService.SendMessage``1(System.Int32,``0,LiteNetLib.DeliveryMethod)')
  - [SentMessageToAll\`\`1()](#M-Framework-Network-NetworkService-SentMessageToAll``1-``0,LiteNetLib-DeliveryMethod- 'Framework.Network.NetworkService.SentMessageToAll``1(``0,LiteNetLib.DeliveryMethod)')
- [Player](#T-Framework-Game-Player 'Framework.Game.Player')
  - [GameWorld](#P-Framework-Game-Player-GameWorld 'Framework.Game.Player.GameWorld')
  - [Id](#P-Framework-Game-Player-Id 'Framework.Game.Player.Id')
  - [Latency](#P-Framework-Game-Player-Latency 'Framework.Game.Player.Latency')
  - [PlayerName](#P-Framework-Game-Player-PlayerName 'Framework.Game.Player.PlayerName')
  - [PreviousState](#P-Framework-Game-Player-PreviousState 'Framework.Game.Player.PreviousState')
  - [State](#P-Framework-Game-Player-State 'Framework.Game.Player.State')
  - [Team](#P-Framework-Game-Player-Team 'Framework.Game.Player.Team')
- [ServerLogic\`1](#T-Framework-Game-Server-ServerLogic`1 'Framework.Game.Server.ServerLogic`1')
  - [NetworkPort](#F-Framework-Game-Server-ServerLogic`1-NetworkPort 'Framework.Game.Server.ServerLogic`1.NetworkPort')
  - [LoadWorld(mapName,worldTick)](#M-Framework-Game-Server-ServerLogic`1-LoadWorld-System-String,System-UInt32- 'Framework.Game.Server.ServerLogic`1.LoadWorld(System.String,System.UInt32)')
  - [OnMapDestroy()](#M-Framework-Game-Server-ServerLogic`1-OnMapDestroy 'Framework.Game.Server.ServerLogic`1.OnMapDestroy')
  - [OnMapInstance(res,worldTick)](#M-Framework-Game-Server-ServerLogic`1-OnMapInstance-Godot-PackedScene,System-UInt32- 'Framework.Game.Server.ServerLogic`1.OnMapInstance(Godot.PackedScene,System.UInt32)')
  - [_EnterTree()](#M-Framework-Game-Server-ServerLogic`1-_EnterTree 'Framework.Game.Server.ServerLogic`1._EnterTree')
- [ServerNetworkService](#T-Framework-Network-Services-ServerNetworkService 'Framework.Network.Services.ServerNetworkService')
  - [Bind(port)](#M-Framework-Network-Services-ServerNetworkService-Bind-System-Int32- 'Framework.Network.Services.ServerNetworkService.Bind(System.Int32)')
  - [GetConnectedPeers()](#M-Framework-Network-Services-ServerNetworkService-GetConnectedPeers 'Framework.Network.Services.ServerNetworkService.GetConnectedPeers')
  - [GetConnectionCount()](#M-Framework-Network-Services-ServerNetworkService-GetConnectionCount 'Framework.Network.Services.ServerNetworkService.GetConnectionCount')
  - [Register()](#M-Framework-Network-Services-ServerNetworkService-Register 'Framework.Network.Services.ServerNetworkService.Register')
  - [Unregister()](#M-Framework-Network-Services-ServerNetworkService-Unregister 'Framework.Network.Services.ServerNetworkService.Unregister')
- [ServerWorld](#T-Framework-Game-Server-ServerWorld 'Framework.Game.Server.ServerWorld')
  - [BroadcastWorldHearbeat()](#M-Framework-Game-Server-ServerWorld-BroadcastWorldHearbeat-System-Single- 'Framework.Game.Server.ServerWorld.BroadcastWorldHearbeat(System.Single)')
  - [OnPlayerConnected(clientId)](#M-Framework-Game-Server-ServerWorld-OnPlayerConnected-System-Int32- 'Framework.Game.Server.ServerWorld.OnPlayerConnected(System.Int32)')
  - [OnPlayerDisconnect(clientId)](#M-Framework-Game-Server-ServerWorld-OnPlayerDisconnect-System-Int32,LiteNetLib-DisconnectReason- 'Framework.Game.Server.ServerWorld.OnPlayerDisconnect(System.Int32,LiteNetLib.DisconnectReason)')
  - [OnPlayerInput(package,peer)](#M-Framework-Game-Server-ServerWorld-OnPlayerInput-Framework-Network-Commands-PlayerInputCommand,LiteNetLib-NetPeer- 'Framework.Game.Server.ServerWorld.OnPlayerInput(Framework.Network.Commands.PlayerInputCommand,LiteNetLib.NetPeer)')
  - [_EnterTree()](#M-Framework-Game-Server-ServerWorld-_EnterTree 'Framework.Game.Server.ServerWorld._EnterTree')
- [SpawnPoint](#T-Framework-Game-SpawnPoint 'Framework.Game.SpawnPoint')
  - [inUsage](#F-Framework-Game-SpawnPoint-inUsage 'Framework.Game.SpawnPoint.inUsage')
- [StringDictonary\`1](#T-Framework-Extensions-StringDictonary`1 'Framework.Extensions.StringDictonary`1')
  - [All](#P-Framework-Extensions-StringDictonary`1-All 'Framework.Extensions.StringDictonary`1.All')
  - [Create\`\`1()](#M-Framework-Extensions-StringDictonary`1-Create``1-System-String- 'Framework.Extensions.StringDictonary`1.Create``1(System.String)')
  - [Get\`\`1()](#M-Framework-Extensions-StringDictonary`1-Get``1-System-String- 'Framework.Extensions.StringDictonary`1.Get``1(System.String)')
  - [TryGetService()](#M-Framework-Extensions-StringDictonary`1-TryGetService-System-String,`0@- 'Framework.Extensions.StringDictonary`1.TryGetService(System.String,`0@)')
- [TypeDictonary\`1](#T-Framework-TypeDictonary`1 'Framework.TypeDictonary`1')
  - [All](#P-Framework-TypeDictonary`1-All 'Framework.TypeDictonary`1.All')
  - [Create\`\`1()](#M-Framework-TypeDictonary`1-Create``1 'Framework.TypeDictonary`1.Create``1')
  - [Get\`\`1()](#M-Framework-TypeDictonary`1-Get``1 'Framework.TypeDictonary`1.Get``1')
  - [TryGetService()](#M-Framework-TypeDictonary`1-TryGetService-System-Type,`0@- 'Framework.TypeDictonary`1.TryGetService(System.Type,`0@)')

<a name='T-Framework-Utils-AsyncLoader'></a>
## AsyncLoader `type`

##### Namespace

Framework.Utils

##### Summary

Helper class to load resources in background

<a name='M-Framework-Utils-AsyncLoader-Tick'></a>
### Tick() `method`

##### Summary

Brings the async loader to the next state.
Binded on your _Process Method

##### Parameters

This method has no parameters.

<a name='T-Framework-Utils-CircularBuffer`1'></a>
## CircularBuffer\`1 `type`

##### Namespace

Framework.Utils

##### Summary

*Inherit from parent.*

##### Summary

Circular buffer.

When writing to a full buffer:
PushBack -> removes this[0] / Front()
PushFront -> removes this[Size-1] / Back()

this implementation is inspired by
http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html
because I liked their interface.

<a name='M-Framework-Utils-CircularBuffer`1-#ctor-System-Int32,`0[]-'></a>
### #ctor(capacity,items) `constructor`

##### Summary

Initializes a new instance of the [CircularBuffer\`1](#T-Framework-Utils-CircularBuffer`1 'Framework.Utils.CircularBuffer`1') class.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| capacity | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Buffer capacity. Must be positive. |
| items | [\`0[]](#T-`0[] '`0[]') | Items to fill buffer with. Items length must be less than capacity.
Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
any enumerable. |

<a name='F-Framework-Utils-CircularBuffer`1-_end'></a>
### _end `constants`

##### Summary

The _end. Index after the last element in the buffer.

<a name='F-Framework-Utils-CircularBuffer`1-_size'></a>
### _size `constants`

##### Summary

The _size. Buffer size.

<a name='F-Framework-Utils-CircularBuffer`1-_start'></a>
### _start `constants`

##### Summary

The _start. Index of the first element in buffer.

<a name='P-Framework-Utils-CircularBuffer`1-Capacity'></a>
### Capacity `property`

##### Summary

Maximum capacity of the buffer. Elements pushed into the buffer after
maximum capacity is reached (IsFull = true), will remove an element.

<a name='P-Framework-Utils-CircularBuffer`1-Size'></a>
### Size `property`

##### Summary

Current buffer size (the number of elements that the buffer has).

<a name='M-Framework-Utils-CircularBuffer`1-Back'></a>
### Back() `method`

##### Summary

Element at the back of the buffer - this[Size - 1].

##### Returns

The value of the element of type T at the back of the buffer.

##### Parameters

This method has no parameters.

<a name='M-Framework-Utils-CircularBuffer`1-Decrement-System-Int32@-'></a>
### Decrement(index) `method`

##### Summary

Decrements the provided index variable by one, wrapping
around if necessary.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| index | [System.Int32@](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32@ 'System.Int32@') |  |

<a name='M-Framework-Utils-CircularBuffer`1-Front'></a>
### Front() `method`

##### Summary

Element at the front of the buffer - this[0].

##### Returns

The value of the element of type T at the front of the buffer.

##### Parameters

This method has no parameters.

<a name='M-Framework-Utils-CircularBuffer`1-Increment-System-Int32@-'></a>
### Increment(index) `method`

##### Summary

Increments the provided index variable by one, wrapping
around if necessary.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| index | [System.Int32@](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32@ 'System.Int32@') |  |

<a name='M-Framework-Utils-CircularBuffer`1-InternalIndex-System-Int32-'></a>
### InternalIndex(index) `method`

##### Summary

Converts the index in the argument to an index in

```
_buffer
```

##### Returns

The transformed index.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| index | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | External index. |

<a name='M-Framework-Utils-CircularBuffer`1-PopBack'></a>
### PopBack() `method`

##### Summary

Removes the element at the back of the buffer. Decreasing the 
Buffer size by 1.

##### Parameters

This method has no parameters.

<a name='M-Framework-Utils-CircularBuffer`1-PopFront'></a>
### PopFront() `method`

##### Summary

Removes the element at the front of the buffer. Decreasing the 
Buffer size by 1.

##### Parameters

This method has no parameters.

<a name='M-Framework-Utils-CircularBuffer`1-PushBack-`0-'></a>
### PushBack(item) `method`

##### Summary

Pushes a new element to the back of the buffer. Back()/this[Size-1]
will now return this element.

When the buffer is full, the element at Front()/this[0] will be 
popped to allow for this new element to fit.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| item | [\`0](#T-`0 '`0') | Item to push to the back of the buffer |

<a name='M-Framework-Utils-CircularBuffer`1-PushFront-`0-'></a>
### PushFront(item) `method`

##### Summary

Pushes a new element to the front of the buffer. Front()/this[0]
will now return this element.

When the buffer is full, the element at Back()/this[Size-1] will be 
popped to allow for this new element to fit.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| item | [\`0](#T-`0 '`0') | Item to push to the front of the buffer |

<a name='M-Framework-Utils-CircularBuffer`1-ToArray'></a>
### ToArray() `method`

##### Summary

Copies the buffer contents to an array, according to the logical
contents of the buffer (i.e. independent of the internal 
order/contents)

##### Returns

A new array with a copy of the buffer contents.

##### Parameters

This method has no parameters.

<a name='T-Framework-Game-Client-ClientWorld'></a>
## ClientWorld `type`

##### Namespace

Framework.Game.Client

<a name='F-Framework-Game-Client-ClientWorld-clientSimulationAdjuster'></a>
### clientSimulationAdjuster `constants`

##### Summary

Client adjuster
Handles server ticks and make them accuracy

<a name='F-Framework-Game-Client-ClientWorld-lastServerWorldTick'></a>
### lastServerWorldTick `constants`

##### Summary

Last executed server tick

<a name='F-Framework-Game-Client-ClientWorld-localPlayer'></a>
### localPlayer `constants`

##### Summary

Local player class

<a name='F-Framework-Game-Client-ClientWorld-replayedStates'></a>
### replayedStates `constants`

##### Summary

Last replayed states

<a name='F-Framework-Game-Client-ClientWorld-worldStateQueue'></a>
### worldStateQueue `constants`

##### Summary

World Heartbeat Queue

##### Returns



##### Generic Types

| Name | Description |
| ---- | ----------- |
| WorldHeartbeat |  |

<a name='M-Framework-Game-Client-ClientWorld-ExecuteHeartbeat-Framework-Network-Commands-WorldHeartbeat-'></a>
### ExecuteHeartbeat(update) `method`

##### Summary

Execute world heartbeat to initalize players and set values

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| update | [Framework.Network.Commands.WorldHeartbeat](#T-Framework-Network-Commands-WorldHeartbeat 'Framework.Network.Commands.WorldHeartbeat') |  |

<a name='M-Framework-Game-Client-ClientWorld-PostUpdate'></a>
### PostUpdate(interval) `method`

##### Summary

After each physical frame

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| interval | [M:Framework.Game.Client.ClientWorld.PostUpdate](#T-M-Framework-Game-Client-ClientWorld-PostUpdate 'M:Framework.Game.Client.ClientWorld.PostUpdate') |  |

<a name='M-Framework-Game-Client-ClientWorld-ProcessServerWorldState-Framework-Network-Commands-WorldHeartbeat-'></a>
### ProcessServerWorldState(incomingState) `method`

##### Summary

Compare inputs with current state and do client interpolation

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| incomingState | [Framework.Network.Commands.WorldHeartbeat](#T-Framework-Network-Commands-WorldHeartbeat 'Framework.Network.Commands.WorldHeartbeat') |  |

<a name='M-Framework-Game-Client-ClientWorld-Tick-System-Single-'></a>
### Tick(interval) `method`

##### Summary

Handle client tick

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| interval | [System.Single](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Single 'System.Single') |  |

<a name='T-Framework-ComponentRegistry'></a>
## ComponentRegistry `type`

##### Namespace

Framework

##### Summary

Component registry class

<a name='M-Framework-ComponentRegistry-#ctor-Framework-IBaseComponent-'></a>
### #ctor(baseComponent) `constructor`

##### Summary

Create a component registry by given base component

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| baseComponent | [Framework.IBaseComponent](#T-Framework-IBaseComponent 'Framework.IBaseComponent') | The base component |

<a name='P-Framework-ComponentRegistry-All'></a>
### All `property`

##### Summary

Get all avaiable components

##### Returns



<a name='M-Framework-ComponentRegistry-AddComponentAsync``1-System-String,System-Action{System-Boolean}-'></a>
### AddComponentAsync\`\`1(resourcePath,callback) `method`

##### Summary

Add an new component to base component by given resource path (async)

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| resourcePath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| callback | [System.Action{System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Boolean}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-ComponentRegistry-AddComponent``1'></a>
### AddComponent\`\`1() `method`

##### Summary

Add an new component to base component

##### Returns



##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-ComponentRegistry-AddComponent``1-System-String-'></a>
### AddComponent\`\`1(resourcePath) `method`

##### Summary

Add an new component to base component by given resource path

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| resourcePath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-ComponentRegistry-DeleteComponent``1'></a>
### DeleteComponent\`\`1() `method`

##### Summary

Delete a given componentn from base component

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-ComponentRegistry-Get``1'></a>
### Get\`\`1() `method`

##### Summary

Get an existing component of the base component

##### Returns



##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-ComponentRegistry-TryGetService-System-Type,Godot-Node@-'></a>
### TryGetService() `method`

##### Summary

*Inherit from parent.*

##### Parameters

This method has no parameters.

<a name='T-Framework-Game-GameLogic'></a>
## GameLogic `type`

##### Namespace

Framework.Game

##### Summary

Basic game logic component

<a name='M-Framework-Game-GameLogic-#ctor'></a>
### #ctor() `constructor`

##### Summary

*Inherit from parent.*

##### Parameters

This constructor has no parameters.

<a name='F-Framework-Game-GameLogic-_serviceRegistry'></a>
### _serviceRegistry `constants`

##### Summary

*Inherit from parent.*

<a name='F-Framework-Game-GameLogic-currentWorld'></a>
### currentWorld `constants`

##### Summary

*Inherit from parent.*

<a name='F-Framework-Game-GameLogic-loader'></a>
### loader `constants`

##### Summary

*Inherit from parent.*

<a name='F-Framework-Game-GameLogic-mapLoadInProcess'></a>
### mapLoadInProcess `constants`

##### Summary

*Inherit from parent.*

<a name='F-Framework-Game-GameLogic-secureConnectionKey'></a>
### secureConnectionKey `constants`

##### Summary

Secure passphrase for network connection

<a name='P-Framework-Game-GameLogic-CurrentWorld'></a>
### CurrentWorld `property`

##### Summary

The active game world

<a name='P-Framework-Game-GameLogic-MapLoader'></a>
### MapLoader `property`

##### Summary

The async loader for eg. maps

##### Returns



<a name='P-Framework-Game-GameLogic-Services'></a>
### Services `property`

##### Summary

Service Registry (Contains all services)

<a name='M-Framework-Game-GameLogic-LoadWorld-System-String,System-UInt32-'></a>
### LoadWorld(path,worldTick) `method`

##### Summary

Load an world by given resource path

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| path | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| worldTick | [System.UInt32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.UInt32 'System.UInt32') |  |

<a name='M-Framework-Game-GameLogic-OnMapDestroy'></a>
### OnMapDestroy() `method`

##### Summary

On destroy map

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-GameLogic-OnMapInstance-Godot-PackedScene,System-UInt32-'></a>
### OnMapInstance() `method`

##### Summary

Instance an map by given PackedScene and WorldTick

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-GameLogic-_EnterTree'></a>
### _EnterTree() `method`

##### Summary

On tree enter

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-GameLogic-_ExitTree'></a>
### _ExitTree() `method`

##### Summary

After exit tree

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-GameLogic-_Process-System-Single-'></a>
### _Process(delta) `method`

##### Summary

Update all regisered services

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| delta | [System.Single](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Single 'System.Single') |  |

<a name='T-Framework-Game-IGameLogic'></a>
## IGameLogic `type`

##### Namespace

Framework.Game

##### Summary

Required interface for game logic

<a name='P-Framework-Game-IGameLogic-Services'></a>
### Services `property`

##### Summary

Service registry (which contains the service of the game logic)

<a name='T-Framework-Game-ILevel'></a>
## ILevel `type`

##### Namespace

Framework.Game

##### Summary

Required interface for levels

<a name='M-Framework-Game-ILevel-GetAllSpawnPoints'></a>
### GetAllSpawnPoints() `method`

##### Summary

List of avaible spawn points

##### Returns



##### Parameters

This method has no parameters.

<a name='T-Framework-Game-IPlayer'></a>
## IPlayer `type`

##### Namespace

Framework.Game

##### Summary

Required interface for players

<a name='P-Framework-Game-IPlayer-Id'></a>
### Id `property`

##### Summary

Id of player

<a name='P-Framework-Game-IPlayer-Latency'></a>
### Latency `property`

##### Summary

Current latency (ping)

<a name='P-Framework-Game-IPlayer-PlayerName'></a>
### PlayerName `property`

##### Summary

Name of player

<a name='P-Framework-Game-IPlayer-State'></a>
### State `property`

##### Summary

Current connection state

<a name='P-Framework-Game-IPlayer-Team'></a>
### Team `property`

##### Summary

Assigned team of player

<a name='T-Framework-IService'></a>
## IService `type`

##### Namespace

Framework

##### Summary

Required interface for service classes

<a name='M-Framework-IService-Register'></a>
### Register() `method`

##### Summary

Register the service

##### Parameters

This method has no parameters.

<a name='M-Framework-IService-Render-System-Single-'></a>
### Render(delta) `method`

##### Summary

Update the service physics

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| delta | [System.Single](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Single 'System.Single') | Delta time of physics processs |

<a name='M-Framework-IService-Unregister'></a>
### Unregister() `method`

##### Summary

Unregister the service

##### Parameters

This method has no parameters.

<a name='M-Framework-IService-Update-System-Single-'></a>
### Update(delta) `method`

##### Summary

Update the service

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| delta | [System.Single](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Single 'System.Single') | Delta time of none-physics processs |

<a name='T-Framework-Game-IWorld'></a>
## IWorld `type`

##### Namespace

Framework.Game

##### Summary

The required interface for an game world

<a name='P-Framework-Game-IWorld-Level'></a>
### Level `property`

##### Summary

The loaded game level of the world

<a name='P-Framework-Game-IWorld-Players'></a>
### Players `property`

##### Summary

All players of the world

<a name='P-Framework-Game-IWorld-ServerVars'></a>
### ServerVars `property`

##### Summary

The server vars of the world

<a name='P-Framework-Game-IWorld-WorldTick'></a>
### WorldTick `property`

##### Summary

The current tick of the world

<a name='M-Framework-Game-IWorld-Destroy'></a>
### Destroy() `method`

##### Summary

Destroy the game world

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-IWorld-Init-System-Collections-Generic-Dictionary{System-String,System-String},System-UInt32-'></a>
### Init(serverVars,initalWorldTick) `method`

##### Summary

Init the server world (first time)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serverVars | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | List of avaible server vars |
| initalWorldTick | [System.UInt32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.UInt32 'System.UInt32') | Staring game time |

<a name='T-Framework-Game-Level'></a>
## Level `type`

##### Namespace

Framework.Game

##### Summary

A basic class for an game level

<a name='M-Framework-Game-Level-GetAllSpawnPoints'></a>
### GetAllSpawnPoints() `method`

##### Summary

*Inherit from parent.*

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-Level-GetFreeSpawnPoints'></a>
### GetFreeSpawnPoints() `method`

##### Summary

*Inherit from parent.*

##### Parameters

This method has no parameters.

<a name='T-Framework-Physics-MovementCalculator'></a>
## MovementCalculator `type`

##### Namespace

Framework.Physics

<a name='M-Framework-Physics-MovementCalculator-AirControl-Godot-Vector3,System-Single,System-Single-'></a>
### AirControl() `method`

##### Parameters

This method has no parameters.

<a name='M-Framework-Physics-MovementCalculator-AirMove-System-Single-'></a>
### AirMove() `method`

##### Parameters

This method has no parameters.

<a name='M-Framework-Physics-MovementCalculator-ApplyFriction-System-Single,System-Single-'></a>
### ApplyFriction() `method`

##### Parameters

This method has no parameters.

<a name='T-Framework-Network-NetworkService'></a>
## NetworkService `type`

##### Namespace

Framework.Network

<a name='F-Framework-Network-NetworkService-_netPacketProcessor'></a>
### _netPacketProcessor `constants`

##### Summary

Default server port

<a name='M-Framework-Network-NetworkService-SendMessageSerialisable``1-System-Int32,``0,LiteNetLib-DeliveryMethod-'></a>
### SendMessageSerialisable\`\`1(peerId,command,method) `method`

##### Summary

Send an network message to an specific client by id (serialized)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| peerId | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| command | [\`\`0](#T-``0 '``0') |  |
| method | [LiteNetLib.DeliveryMethod](#T-LiteNetLib-DeliveryMethod 'LiteNetLib.DeliveryMethod') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-Network-NetworkService-SendMessage``1-System-Int32,``0,LiteNetLib-DeliveryMethod-'></a>
### SendMessage\`\`1(peerId,obj,method) `method`

##### Summary

Send an message to an specific client (non serialized)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| peerId | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| obj | [\`\`0](#T-``0 '``0') |  |
| method | [LiteNetLib.DeliveryMethod](#T-LiteNetLib-DeliveryMethod 'LiteNetLib.DeliveryMethod') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-Framework-Network-NetworkService-SentMessageToAll``1-``0,LiteNetLib-DeliveryMethod-'></a>
### SentMessageToAll\`\`1() `method`

##### Summary

Send an network message to all clients

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-Framework-Game-Player'></a>
## Player `type`

##### Namespace

Framework.Game

##### Summary

The general player class

<a name='P-Framework-Game-Player-GameWorld'></a>
### GameWorld `property`

##### Summary

*Inherit from parent.*

<a name='P-Framework-Game-Player-Id'></a>
### Id `property`

##### Summary

*Inherit from parent.*

<a name='P-Framework-Game-Player-Latency'></a>
### Latency `property`

##### Summary

*Inherit from parent.*

<a name='P-Framework-Game-Player-PlayerName'></a>
### PlayerName `property`

##### Summary

*Inherit from parent.*

<a name='P-Framework-Game-Player-PreviousState'></a>
### PreviousState `property`

##### Summary

Previous state since last tick

<a name='P-Framework-Game-Player-State'></a>
### State `property`

##### Summary

*Inherit from parent.*

<a name='P-Framework-Game-Player-Team'></a>
### Team `property`

##### Summary

*Inherit from parent.*

<a name='T-Framework-Game-Server-ServerLogic`1'></a>
## ServerLogic\`1 `type`

##### Namespace

Framework.Game.Server

<a name='F-Framework-Game-Server-ServerLogic`1-NetworkPort'></a>
### NetworkPort `constants`

##### Summary

Network server port

<a name='M-Framework-Game-Server-ServerLogic`1-LoadWorld-System-String,System-UInt32-'></a>
### LoadWorld(mapName,worldTick) `method`

##### Summary

Loading the game world (async)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| worldTick | [System.UInt32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.UInt32 'System.UInt32') |  |

<a name='M-Framework-Game-Server-ServerLogic`1-OnMapDestroy'></a>
### OnMapDestroy() `method`

##### Summary

Destroy the game world

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-Server-ServerLogic`1-OnMapInstance-Godot-PackedScene,System-UInt32-'></a>
### OnMapInstance(res,worldTick) `method`

##### Summary

Instancing the game world

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| res | [Godot.PackedScene](#T-Godot-PackedScene 'Godot.PackedScene') |  |
| worldTick | [System.UInt32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.UInt32 'System.UInt32') |  |

<a name='M-Framework-Game-Server-ServerLogic`1-_EnterTree'></a>
### _EnterTree() `method`

##### Summary

Instance the server game logic and load the map from default settings

##### Parameters

This method has no parameters.

<a name='T-Framework-Network-Services-ServerNetworkService'></a>
## ServerNetworkService `type`

##### Namespace

Framework.Network.Services

<a name='M-Framework-Network-Services-ServerNetworkService-Bind-System-Int32-'></a>
### Bind(port) `method`

##### Summary

Bind server on specific port

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| port | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |

<a name='M-Framework-Network-Services-ServerNetworkService-GetConnectedPeers'></a>
### GetConnectedPeers() `method`

##### Summary

Get all connected clients

##### Returns



##### Parameters

This method has no parameters.

<a name='M-Framework-Network-Services-ServerNetworkService-GetConnectionCount'></a>
### GetConnectionCount() `method`

##### Summary

Get count of current active connections

##### Returns



##### Parameters

This method has no parameters.

<a name='M-Framework-Network-Services-ServerNetworkService-Register'></a>
### Register() `method`

##### Summary

Register the service

##### Parameters

This method has no parameters.

<a name='M-Framework-Network-Services-ServerNetworkService-Unregister'></a>
### Unregister() `method`

##### Summary

Unregister the service

##### Parameters

This method has no parameters.

<a name='T-Framework-Game-Server-ServerWorld'></a>
## ServerWorld `type`

##### Namespace

Framework.Game.Server

<a name='M-Framework-Game-Server-ServerWorld-BroadcastWorldHearbeat-System-Single-'></a>
### BroadcastWorldHearbeat() `method`

##### Summary

Send an heartbeat to all players
Hearbeat contains player informations, server latency, states, etc

##### Parameters

This method has no parameters.

<a name='M-Framework-Game-Server-ServerWorld-OnPlayerConnected-System-Int32-'></a>
### OnPlayerConnected(clientId) `method`

##### Summary

Event called after client is connected to server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientId | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |

<a name='M-Framework-Game-Server-ServerWorld-OnPlayerDisconnect-System-Int32,LiteNetLib-DisconnectReason-'></a>
### OnPlayerDisconnect(clientId) `method`

##### Summary

Event called after client is disconnected from server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientId | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |

<a name='M-Framework-Game-Server-ServerWorld-OnPlayerInput-Framework-Network-Commands-PlayerInputCommand,LiteNetLib-NetPeer-'></a>
### OnPlayerInput(package,peer) `method`

##### Summary

Enqueue new player input

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| package | [Framework.Network.Commands.PlayerInputCommand](#T-Framework-Network-Commands-PlayerInputCommand 'Framework.Network.Commands.PlayerInputCommand') |  |
| peer | [LiteNetLib.NetPeer](#T-LiteNetLib-NetPeer 'LiteNetLib.NetPeer') |  |

<a name='M-Framework-Game-Server-ServerWorld-_EnterTree'></a>
### _EnterTree() `method`

##### Summary

Instancing the server game world

##### Parameters

This method has no parameters.

<a name='T-Framework-Game-SpawnPoint'></a>
## SpawnPoint `type`

##### Namespace

Framework.Game

##### Summary

The gernal spawn point class

<a name='F-Framework-Game-SpawnPoint-inUsage'></a>
### inUsage `constants`

##### Summary

Set or get the usage of the spawnpoint

<a name='T-Framework-Extensions-StringDictonary`1'></a>
## StringDictonary\`1 `type`

##### Namespace

Framework.Extensions

##### Summary

Service registry /service holder of component

<a name='P-Framework-Extensions-StringDictonary`1-All'></a>
### All `property`

##### Summary

Get a full list of registered services

##### Returns



<a name='M-Framework-Extensions-StringDictonary`1-Create``1-System-String-'></a>
### Create\`\`1() `method`

##### Summary

Create and register service by given type

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Service type |

<a name='M-Framework-Extensions-StringDictonary`1-Get``1-System-String-'></a>
### Get\`\`1() `method`

##### Summary

Get an registered service by given type

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Type of service |

<a name='M-Framework-Extensions-StringDictonary`1-TryGetService-System-String,`0@-'></a>
### TryGetService() `method`

##### Summary

*Inherit from parent.*

##### Parameters

This method has no parameters.

<a name='T-Framework-TypeDictonary`1'></a>
## TypeDictonary\`1 `type`

##### Namespace

Framework

##### Summary

Service registry /service holder of component

<a name='P-Framework-TypeDictonary`1-All'></a>
### All `property`

##### Summary

Get a full list of registered services

##### Returns



<a name='M-Framework-TypeDictonary`1-Create``1'></a>
### Create\`\`1() `method`

##### Summary

Create and register service by given type

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Service type |

<a name='M-Framework-TypeDictonary`1-Get``1'></a>
### Get\`\`1() `method`

##### Summary

Get an registered service by given type

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Type of service |

<a name='M-Framework-TypeDictonary`1-TryGetService-System-Type,`0@-'></a>
### TryGetService() `method`

##### Summary

*Inherit from parent.*

##### Parameters

This method has no parameters.
