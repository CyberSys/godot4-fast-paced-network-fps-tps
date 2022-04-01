@echo off
:: --HAS ENDING BACKSLASH
set batdir=%~dp0
:: --MISSING ENDING BACKSLASH
:: set batdir=%CD%
pushd "%batdir%"
echo Your current dir is %batdir%
dotnet msbuild ./Example/Shooter.sln  /restore /t:Build /p:Configuration=Debug /v:normal /p:GodotTargetPlatform=windows
D:\godot\bin\godot.windows.opt.tools.64.mono.exe --path ./Example -client