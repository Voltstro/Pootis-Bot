# Installation

## Computer Requirements

**OS:** Pootis-Bot will run on Windows, MacOS or Linux. It will require a x64 bit machine, as well as .NET Core 3.1.

**RAM:** You will need about 60+ MBs of RAM, this will depend on which features you have enabled, and how many servers and users the bot will be serving.

**Storage:** Base Pootis-Bot is about 7MBs, however the server and users list files can grow quite large, depending on what resource formatting mode you use, how many servers the bot will be in, and how many users are in each server.

**CPU:** Any modern-ish CPU should run the bot, it will depend on which features you have enabled, and how many servers and users the bot will be serving.

It is recommend to run this bot on a computer that can be up 24/7, so a [VPS hosting service](https://www.google.com/search?q=vps+hosting) may not be bad if do don't have a home server or spare PC. Prices for a VPS will vary depending on specs, and region.

## .NET Core

Since Pootis-Bot is written using .NET Core, you will need to install the runtime.

If you already have .NET Core 3.1 installed, you can [skip this part](#pootis-bot).

### Download

First download [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1). If all you want to do is run the bot (no development), then all you need to install is the .NET Core Runtime, you do not need to install the SDK.

### Install

Its pretty simple to install, just like a normal Windows installer, or if you are on a Unix-based system, follow the instructions on the .NET website.

## Pootis-Bot

### Download

First off, start by [downloading](../../download/) the latest version of Pootis-Bot for your OS.

The downloaded `.zip` file should look like `pootis-bot-[os]-[architecture]-[version].zip`. So for a Windows 64 bit version of Pootis-Bot, it should look like `pootis-bot-win-x64-1.0.0.zip`. **NOTE**: The version number will change depending on how many releases have been made since this was written.

Once you have downloaded it, extract the downloaded `.zip` file.

### Install

Pootis-Bot is essentially a portable app, though you want to place the Pootis-Bot files some were you can access easily, and some were without admin or root permissions.

Once you have chosen a place to put the files, you can run Pootis-Bot for the first time. If you are on Windows you can start the bot via running the `Pootis-Bot.exe` application.

If you are not on Windows, then what you need to do is to open up a terminal to were the files are located and run the command `dotnet Pootis-Bot.dll`, this will run the bot.

On first run it will put you straight into the config menu, you will need to set a [Discord API token](../token/). You might want to also set the bot's name, and prefix, as well as setup any other APIs you want to use.

You can read the [Config Menu](../config-menu/) section to learn more.