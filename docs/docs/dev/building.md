# Building Pootis-Bot

These instructions should help you build Pootis-Bot. If you already know how to build for Dotnet Core then you should be fine and probably don't have to read this. 

### Prerequisites

```bat
.NET Core 3.1 SDK
Powershell Core
```

## Build with the scripts

First head to the main directory of Pootis-Bot. Where the `Pootis-Bot.sln` file is. Then go into the `publish-scripts` folder.

In this folder you should see 4 scripts:

- `Publish.ps1`
- `Publish-Linux.sh`
- `Publish-MacOs.sh`
- `Publish-Windows.bat`

You can run the script for your platform to build it.

Example:

Building for Windows 64-Bit you would run the `Publish-Windows.bat` file. Pretty simple, yea?

## How to build with command line

This is just standard Dot.NET Core publish command.

Navigate to where the `Pootis-Bot.csproj` (or the `.sln`) file is.

Open the terminal and run the command:

```batch
dotnet publish -c Release -r [RID]
```