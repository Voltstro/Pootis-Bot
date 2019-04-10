@echo off
title Building for Linux x64
dotnet publish ../Pootis-Bot.sln -c Release -r linux-x64
echo
echo Build complete @ Pootis-Bot\bin\Release\netcoreapp2.2\linux-x64\publish
pause