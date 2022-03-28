# ♨️ The game framework your mother warned you about. 

Godot 4 Fast-Paced (Authoritative Server) Game + Network Framework for FPS and TPS Games

Its in process (extensions incoming)!
In the end it should be a complete framework including an open-source shooter. Including models.
Totaly written in c#. No gdscript, because its sucks hard for oop.

Check [documentation here](Docs/Manual/Framework.md)

# Network features
- Client-side prediction of player entities
- Client-side interpolation of remote entities
- Backwards reconciliation and replay
- Real-time adjustment of client simulation speed to optimize server's input buffer (Overwatch's method).
- Server-side lag compensation
- Full godot server implementation with disabled 3d
- Master and multi clients in one project (split screen)
- Optimized netcode (Quake, Overwatch, Valve methods)
- Remote de(activation) of player components
- Ready to use godot nodes (ex.  ServerPlayer,  ServerWorld, ServerLogic..)
- Server variable sharing between server and client (ServerVars)
- (TODO) RCON Implementation for Server Management

# Physics
- Full implemented TPS and FPS Movement (Quake style)
- Crouching
- Customizeable and extendable movement
- (TODO) Shifting
- (TODO) Lieing
- (TODO) Animation network sync

# Extensions
- (TODO) Hitscan weapons
- (TODO) Projectile weapons
- (TODO) Weapon Handler
- (TODO) Vehicle Handler
- (TODO) Bot Handler 

# Structure
- ServerLogic (Viewport) => Root Node for Server
   - ServerWorld => For handling map and players
      - ServerLevel => Contains your level asset
      - ServerGameRule => Contains the game rules logics
      - ServerPlayer -> Server player class
- ClientLogic (Viewport) => Root Node for Client
   - ClientWorld => For handling map and players
      - ClientLevel => Contains your level asset
      - LocalPlayer => Contains the local player
      - PuppetPlayer => Contains the puppet player

# Helpers
- Component system (for extending characters and game world)
- Registration services (Full threaded services like Networking)
- Async world loader
- (TODO) Async mesh loader

# Utils
- Simple logging

![Network Test preview](https://i.ibb.co/CKZ8nLj/net-preview.png "Network Test preview")

