# Config File

Here is a overview of the config file.

This is for config version: 13, Pootis-Bot version: 1.1.0. Updated 14/10/2020.

## Default config

``` json
{
  "ConfigVersion": "13",
  "BotName": "CSharp Bot",
  "BotPrefix": "$",
  "BotToken": "",
  "TwitchStreamingSite": "https://www.twitch.tv/Voltstro",
  "CheckConnectionStatus": true,
  "CheckConnectionStatusInterval": 60000,
  "DefaultGameMessage": "Use $help for help.",
  "LevelUpAmount": 0,
  "LevelUpCooldown": 15,
  "ReportErrorsToOwner": false,
  "ReportGuildEventsToOwner": false,
  "ResourceFilesFormatting": 1,
  "ApiKeys": {
    "ApiGiphyKey": null,
    "ApiGoogleSearchKey": null,
    "GoogleSearchEngineId": null,
    "ApiSteamKey": null,
    "YouTubeService": false
  },
  "AudioSettings": {
    "AudioServicesEnabled": false,
    "LogPlayStopSongToConsole": true,
    "ExternalDirectory": "External/",
    "MaxVideoTime": "00:07:00",
    "MusicFolderLocation": "Music/",
    "MusicFileFormat": 0
  },
  "VoteSettings": {
    "MaxVoteTime": "7.00:00:00",
    "MaxRunningVotesPerGuild": 3
  }
}
```

## Notice

A lot of these options are in the bot's config menu, If you want to access the bot's config menu, run the command `config` in the console.

## What does each key mean?

### General Options

**ConfigVersion** - The config version, its best to leave this alone since it is only used if the bot's config is updated and it needs to be saved again.

**BotName** - The bot's name, you can set this in the config menu.

**BotPrefix** - What prefix to use, you can set this in the config menu.

**BotToken** - The bot's token, you can set this in the config menu.

**CheckConnectionStatus** - Checks connection status every so often.

**CheckConnectionStatusInterval** - How often to check the connection status, in milliseconds.

**TwitchStreamingSite** - If bot is in streaming mode, what Twitch site should it show, by default it set to mine.

**DefaultGameMessage** - Default game message, in case the bot restarts it will default to this.

**LevelUpCooldown** - How many seconds between each XP given should we wait.

**LevelUpAmount** - How much XP to give.

**ReportErrorsToOwner** - Should the bot owner receive dms about errors.

**ReportGuildEventsToOwner** - Should events such as when the bot joins/leaves a guild get dm about to the bot owner or not

**ResourceFilesFormatting** - Should the resource files (UserAccounts.json/ServerList.json) be indented or not, 1 for indented, 0 for not indented. (Saves on space but is harder to read)

[**AudioSettings**](#audio-settings) - Settings related to music/audio.

[**ApiKeys**](#api-keys) - Settings related to APIs.

[**VoteSettings**](#vote-settings) - Settings related to voting.

### API Keys

All of these can be set in the config menu under `apis`.

**ApiGiphyKey** - Giphy api key.

**ApiGoogleSearchKey** - Google Search api key.

**GoogleSearchEngineID** - Google Search Engine ID.

**ApiSteamKey** - Steam API key.

**YouTubeService** - If YouTube APIs are enabled or not.

### Audio Settings

Audio related settings, all of this should get correctly set-up depending on your os you are running Pootis-Bot from.

**AudioServicesEnabled** - Are the audio services enabled.

**LogPlayStopSongToConsole** - Logs to the console when the bot starts or stops playing a song.

**ExternalDirectory** - The directory where our external applications will live

**MaxVideoTime** - The max video time Pootis-Bot will download, stops people from downloading 10 hour loops and such.

**MusicFolderLocation** - The location of the music folder

**MusicFileFormat** - What file formate we should use (Currently .m03 is only available)

### Vote Settings

Settings that can be used to control how votes work. 

**MaxVoteTime** - Whats the max time a vote can run for

**MaxRunningVotesPerGuild** - How many votes can be running at one time per guild