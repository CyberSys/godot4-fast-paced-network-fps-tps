@echo off
:: --HAS ENDING BACKSLASH
set batdir=%~dp0
:: --MISSING ENDING BACKSLASH
:: set batdir=%CD%
pushd "%batdir%"
echo Your current dir is %batdir%
%batdir%\Editor\windows\godot.windows.opt.tools.64.mono.exe --editor --path ./Example -client