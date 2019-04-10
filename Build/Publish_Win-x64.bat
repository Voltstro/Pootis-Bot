@echo off
title Building for Windows x64
dotnet publish ../Pootis-Bot.sln -c Release -r win-x64
echo
echo Build complete @ Pootis-Bot\bin\Release\netcoreapp2.2\win-x64\publish
pause