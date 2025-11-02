# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [2.0.0] - Unreleased

Pootis-Bot v2 is a complete re-write of Pootis. A lot features were either changed or removed.

## [1.2.0] - 2022-03-14

### Changed

- [Update to Discord.Net 3.x](https://github.com/Voltstro/Pootis-Bot/commit/a2131594fcadad1af76ad0dfa7eccfe2b8c2d918)
- [Update other packages](https://github.com/Voltstro/Pootis-Bot/commit/ea2de4b8cbaf5e6867f15778af0ab0a5ce4bde02)
    - Microsoft.Extensions.DependencyInjection
    - Serilog.Sinks.Async
    - Serilog.Sinks.Console
    - Serilog.Sinks.File
    - SteamWebAPI2
    - Wiki.Net
    - YoutubeExplode
- [Bump copyright](https://github.com/Voltstro/Pootis-Bot/commit/22e4d07aa885d28c12beb32fe7cbcb868f333f66)

## [1.1.1] - 2021-03-01

### Changed

- Update `Discord.Net` to 2.3.0
- Update `Microsoft.Extensions.DependencyInjection` to 5.0.1
- Update `SteamWebAPI2` to 4.2.7
- Update `YoutubeExplode` to 5.1.9
- License year bump

## [1.1.0] - 2020-11-11

### Changed

- Updated Discord.Net
- Updated setup commands
- Drop support for x86
- Full music support on Linux and MacOS! (finally)
- Updated to .NET Core 3.1
- Don't do self contained builds
- Updated all URLs to account for my name change
- Added Steam API key as an option in the config
- Improved help command usage
- Overhauled the voting service
- Renamed 'Role Gives' to 'Opt Roles'
- Added vote end command
- Show vote end and start date
- Better profile creation date time formatting
- Updated emoji checking/handling
- Updated logger to use Serilog
- Better parsing of arguments
- The help console command outputs a list of all the console commands
- Console commands have a summary
- You can now play music directly from a YouTube URL
- Better setup status warnings
- Updated emoji checking/handling

### Fixed

- Fixed music services
- Fixed a bug with server quick setup and not setting the rule reaction channel correctly
- Fixed/updated server points and XP
- Votes will last over bot sessions

## [1.0.3] - 2020-01-02

### Added

- Added a warning in the setup menu for the command `addvcchannel`

### Changed

- Updated the command `setuprulesmessage`'s summary
- Updated some message outputs for the command `setuprulesmessage`
- In the setup menu for the the Rule Reaction, there is now a link to the message

### Fixed

- Fixed a bug where the `RuleMessageChannelId` wasn't set.

## [1.0.2] - 2019-12-29

### Fixed

- Fixed auto voice channels number being wrong when there are multiple active VC channels
- Fixed `Pootis-Bot.csproj` description

## [1.0.1] - 2019-12-26

### Fixed

- Fixed Tronald Dump API erroring out with `Cannot perform runtime binding on a null reference`

## [1.0.0] - 2019-12-24

Undocumented

## [0.3.5] - 2019-11-02

### Added

- Added server points.
- Added 'role gives'.
- Added `wiki` command.
- Added `randomperson` command.
- Added `requestdata` command.
- Added `top10total` command.
- Added `addxp` command. [BOT OWNER]
- Added `removexp` command. [BOT OWNER]
- Added `throwexception` command. [BOT OWNER]
- Added `guildlist` command. [BOT OWNER]
- Added `leaveguild` command. [BOT OWNER]

### Changed

- All resource files are now capitalised. E.G: `serverlist.json` to `ServerList.json`.
- `accounts.json` was renamed to `UserAccounts.json`. An automatic upgrade will happen if your account file is the old name.
- Seprated HelpModules from the config file to its own `HelpModules.json` file.
- Command Handler won't respond to DMs now.
- Permission roles are now stored as the role's ID, not the name.
- Guild settings are removed from the ServerList when the bot leaves the guild.
- Audio external applications can be configured in the config now. (E.G: Where YouTube-DL/Python is)
- You can disable if logging to the console when a song is started/stopped on a guild.
- You can now add multiple roles at a time with the `perm` command.
- Searching with Google/YouTube/Giphy show a 'searching site' screen now.
- Bot checks if it is still in all guilds that are stored in the ServerList.
- Bot checks if the rule reaction message is deleted and notifies the guild owner if it was.
- Profile now shows how many warnings you have in the guild.
- If you are the owner of the bot, you get a 'badge' on your profile now.

### Fixed

- FFMPEG is now killed when the bot is forced left or there is no one in the voice channel.
- Audio service should now correctly remove all of the special characters.
- Bot correctly leaves all audio channels when the `deletemusic` command is ran.

## [0.3.3] - 2019-09-29

### Added

- Added the `mute` command
- Added anti-spam service
    - Stop anyone with a certain role to ping another role
    - Stop mass pings (If a single user pings a certain threshold % of users)
- You can config the anti-spam services with `setup spam`. Read [more here](https://pootis-bot.creepysin.com/server-setup/anti-spam/).

### Changed

- Public methods all have XML comments
- Added some ascii art on startup, because why not?
- Build scripts have been updated with powershell variants, they now also pack the build into a zip file.
- Fixed stuff according to Resharper

### Fixed

- Bot should leave the audio channel without errors now

## [0.2.9] - 2019-06-29

### Added

- Added the `purge` command
- Added [auto voice channels](https://www.youtube.com/watch?v=5i8Ihz0fqKw)
- Added [Tronald Dump API](https://www.tronalddump.io/), use the command `tronald` to use it
- Levelup cooldown has been added, default 15 seconds

## [0.2.4] - 2019-06-23

### Added

- Added CheckConnectionStatus
- Added Config Version
- Added `status` console command
- Added `getbannedchannels` command
- Added `alluserroles` command
- Added `allroles` command

### Changes

- Tells the user if you don't meet the preconditions
- Tells the user if you don't have the correct args count
- Updated the help command
- cThe owner of the Discord server can override any permission

### Fixed

- Useraccount file doesn't save the user level any more since it is calculated from the points
- The bot checks how many users are in the active audio playing channel and if it is just the bot then it leaves
- You can't add a perm to the `perm` command

## [0.2.0] - 2019-05-10

### Added

- Added the console command `forceaudioupdate`

### Changes

- The bot now downloads python, youtube-dl and ffmpeg
- You can now set the default game message in config.json

### Fixed

- Updated the console command `setgame` so it correctly set if streaming or not when the game is changed
- Removed a debug console.writeline from the audio service
- Made the level up message level number bold

## [0.1.0] - 2019-05-05

### Added

- Initial Release
