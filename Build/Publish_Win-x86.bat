@echo off
title Building for Windows x86
dotnet publish ../Pootis-Bot.sln -c Release -r win-x86
echo
echo Build complete @ Pootis-Bot\bin\Release\netcoreapp2.2\win-x86\publish
pause