@echo off
:: --HAS ENDING BACKSLASH
set batdir=%~dp0
:: --MISSING ENDING BACKSLASH
:: set batdir=%CD%
pushd "%batdir%"
dotnet msbuild ./Example/Shooter.sln  /restore /t:Build /p:Configuration=Debug /v:normal /p:GodotTargetPlatform=windows

echo J | rmdir /s Docs\Manual 
xmldocmd --source https://github.com/TeamStriked/godot4-fast-paced-network-fps-tps/blob/master/Example/Framework --namespace Framework --clean  Example\Framework\bin\Debug\netstandard2.1\Framework.dll Docs\Manual 