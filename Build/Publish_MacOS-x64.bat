@echo off
title Building for MacOSx x64
dotnet publish ../Pootis-Bot.sln -c Release -r osx-x64
echo
echo Build complete @ Pootis-Bot\bin\Release\netcoreapp2.2\osx-x64\publish
pause