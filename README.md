# 🤩 The game framework your mother warned you about. 🤩

Godot 4 Fast-Paced (Authoritative Server) Game + Network Framework for FPS and TPS Games

Its in process (extensions incoming)!
In the end it should be a complete framework including an open-source shooter. Including models.
Totaly written in c#. No gdscript, because its sucks hard for oop.

Check [documentation here](Docs/Manual/Framework.md)

## 🥰 Looking for new Contributors 🥰
- 3D Animators
- 3D Artists
- Level Designer
- C# Developers

Join our discord to support this open source shooter and framework:
https://discord.gg/HW2Mr7Vcep

## Next steps
- Weapon System (in Process)
- Player Animations
- Training World Environment
- Docker image for linux
- Batch scripts for linux

## Usage Keys
- F1 -> Switch between TPS and FPS View
- F5 -> Switch and enable the gui and input for the next client window
- F6 -> Add another client window to the split screen
- ESC -> In game settings menu

## Known Bugs
- Issue with colliding characters or colliding with an wall / edges of an wall (server and client have not the same position, im on research, mby velocity issue..)

## Network features
- Client-side prediction of player entities
- Client-side interpolation of remote entities
- Backwards reconciliation and replay
- Server standalone
- Network Sync Vars
- Real-time adjustment of client simulation speed to optimize server's input buffer (Overwatch's method).
- Server-side lag compensation
- Full godot server implementation with disabled 3d
- Master and multi clients in one project (split screen)
- Optimized netcode (Quake, Overwatch, Valve methods)
- Remote de(activation) of player components
- Ready to use godot nodes (ex.  ServerPlayer,  ServerWorld, ServerLogic..)
- Server variable sharing between server and client (ServerVars)
- RCON Implementation for Server Management
- (TODO) Object syncronistation (dropped guns, etc)

## Example
-  Sound syncronistation for footesteps
-  Body movement for puppet, local and server player
-  Moveable camera
-  Two Startup scenes (Bootloader or BootloaderMutliView)
   - Bootloader is for running the server and client with arguments over shell (see client.cmd and server.cmd)
   - BootloaderMutliView is for running Client, Server and more Client in one Window (For testing)

## Physics
- Full implemented TPS and FPS Movement (Quake style)
- Crouching
- Customizeable and extendable movement
- Animation network sync

## Extensions
- Hitscan weapons
- (TODO) Projectile weapons
- (TODO) Weapon Handler
- (TODO) Vehicle Handler
- (TODO) Bot Handler 

## Structure
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

## Helpers
- Component system (for extending characters and game world)
- Registration services (Full threaded services like Networking)
- Async world loader

## Utils
- Simple logging


![Debug Server View](Docs/Screenshots/DebugServerPosition.jpg "Debug Server View")
![Split Screen](Docs/Screenshots/SplitScreen.jpg "Split Screen")
![Ingame Menu](Docs/Screenshots/IngameMenu.jpg "Ingame Menu")
