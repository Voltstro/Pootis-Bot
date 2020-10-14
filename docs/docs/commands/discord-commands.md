# Discord Commands

This page contains all commands that Pootis-Bot can currently do as of version 1.1.0.

For help with a specific command, do `help [command]`.

Please note more commands may be added in the future, and this list may not get immediately updated! This list will only be kept maintained as of the most recent release of Pootis-Bot!

## General User Commands

### Basic Commands

|Command       |Summary                                                       |Alias|
|--------------|--------------------------------------------------------------|-----|
|`hello`       |Displays the 'hello' message                                  |None |
|`server`      |Gets details about the server you are in                      |None |
|`top10`       |Gets the top 10 users in the server that you are currently in |None |
|`top10total`  |Gets the top users in Pootis-Bot                              |None |

### Help Commands

|Command       |Summary                        |Alias                        |
|--------------|-------------------------------|-----------------------------|
|`help`        |Gets help                      |`h`                          |
|`help [query]`|Gets help on a specific command|`h`, `command`, `chelp`, `ch`|

### Misc Commands

|Command                       |Summary                                            |Alias  |
|------------------------------|---------------------------------------------------|-------|
|`pick [option 1] | [option 2]`|Picks between two things                           |None   |
|`roll [min] [max]`            |Roles between 0 and 50 or between two custom number|None   |
|`reminds`                     |Reminds you, duh (In seconds)                      |`res`  |

### Utils Commands

|Command                         |Summary                                   |Alias  |
|--------------------------------|------------------------------------------|-------|
|`hasrole`                       |Checks if user has a role                 |None   |
|`alluserroles`                  |Gets all roles that a user has            |None   |
|`allroles`                      |Gets all roles on the server              |None   |
|`embedmessage [title] [message]`|Displays your message in an embed message |`embed`|
|`ping`                          |Ping Pong!                                |None   |

### Voting Commands

|Command                                                 |Summary      |Alias  |
|--------------------------------------------------------|-------------|-------|
|`vote [title] [description] [YesEmoji] [NoEmoji] [Time]`|Starts a vote|none   |
|`vote end [VoteID]`                                     |Ends a vote  |none   |

## Fun Commands

|Command                    |Summary                   |Alias                                                   |
|---------------------------|--------------------------|--------------------------------------------------------|
|`youtube [search]`         |Searches YouTube          |`yt`                                                    |
|`google [search]`          |Searches Google           |`g`                                                     |
|`giphy [search]`           |Searches Giphy            |`gy`                                                    |
|`tronald [random|search]`  |Searches Tronald dump API |`tronalddump`, `dump`, `donald`, `donaldtrump`, `trump` |
|`wiki [search]`            |Searches Wikipedia        |`wikipedia`                                             |
|`randomperson`             |Generate a random person  |`person`, `randperson`                                  |

## Steam Commands

|Command              |Summary                                                  |Alias|
|---------------------|---------------------------------------------------------|-----|
|`steam search [user]`|Searches Steam for a user (either an ID, or a vanity URL)|None |

## Account Commands

### Account Utils

|Command               |Summary                                                                              |Alias     |
|----------------------|-------------------------------------------------------------------------------------|----------|
|`profile [user]`      |Gets a profile of you or somebody else.                                              |None      |
|`profile`             |Gets a profile of you or somebody else.                                              |None      |
|`profilemsg [message]`|Sets your custom profile message.                                                    |None      |

### Account Data Management

|Command                    |Summary                                                                |Alias              |
|---------------------------|-----------------------------------------------------------------------|-------------------|
|`requestdata`              |Gives you a .json file of your user profile that Pootis-Bot has of you.|`getdata`, `mydata`|
|`resetprofile`             |Resets your profile (xp, profile message). DOES NOT RESET SERVER DATA! |None               |  

## Audio Commands

### Music

|Command                 |Summary                                      |Alias|
|------------------------|---------------------------------------------|-----|
|`join`					 |Joins in the current voice channel you are in|None |
|`leave`                 |Leaves the current voice channel thats it in |None |
|`play [song]`           |Plays a song                                 |None |
|`stop`                  |Stops the current playing song               |None |
|`pause`                 |Pauses the current song                      |None |

### Auto Voice Channel Commands

|Command                   |Summary                    |Alias|
|--------------------------|---------------------------|-----|
|`addvcchannel [Base Name]`|Adds an auto voice channel |None |

## Server Commands

### Server User Commands

These commands are intended to be used by regular users, however they are server specific as arguments may vary per server.

|Command   |Summary                                                                    |Alias                              |
|----------|---------------------------------------------------------------------------|-----------------------------------|
|`role`    |Gives you an opt role, if it exists and if you meet the conditions (if any)|`get optrole`, `optrole`, `getrole`|
|`optroles`|Gets all opt roles that are on this server                                 |None                               |

### Server Admin Commands

These commands are intended to be used by the server's moderators. The command will require both the bot and the user executing the command the permission of the desired action.

|Command               |Summary              |Alias|
|----------------------|---------------------|-----|
|`kick [user] [reason]`|Kicks a user         |None |
|`ban [user] [reason]` |Bans a user          |None |
|`mute [user]`         |Mutes a user         |None |
|`purge [MessageCount]`|Deletes bulk messages|None |

### Server Spam Setting Commands

These commands can only be run by a guild owner!

|Command                                    |Summary                                                                                        |Alias|
|-------------------------------------------|-----------------------------------------------------------------------------------------------|-----|
|`spam toggle mentionuserspam`              |Enables / Disables the mention user anti-spam feature                                          |None |
|`spam set mentionuserthreshold [threshold]`|Set how much of a percentage of a servers users need to be mention before it is considered spam|None |
|`spam set roletorolewarnings [warnings]`   |Sets how many role to role mention warnings before a proper warning will be given out          |None |

### Server Permission Commands

These commands can only be run by a guild owner!

|Command                          |Summary                                          |Alias                               |
|---------------------------------|-------------------------------------------------|------------------------------------|
|`perm [command] [subCmd] [roles]`|Adds or removes a command's permissions          |None                                |
|`perms`                          |Gets a list of all commands that have permissions|`permissions`, `allperm`, `allperms`|

### Server Setup Commands

All of these commands require a guild owner to execute the command. Commands related to guild owners (such as `setup add guildowner`) can only be executed by the server owner.

|Command                                                                    |Summary                                                                                                   |
|---------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------|
|`setup`                                                                    |Provides basic help for server setup                                                                      |
|`setup status`                                                             |Displays setup info                                                                                       |
|`setup spam`                                                               |Shows setup info regarding the server's anti-spam settings                                                |
|`setup bannedchannels`                                                     |Gets all banned channels                                                                                  |
|`setup add bannedchannel [channel]`                                        |Adds a banned channel                                                                                     |
|`setup remove bannedchannel [channel]`                                     |Removes a banned channel                                                                                  |
|`setup add guildowner [user]`                                              |Adds a guild owner                                                                                        |
|`setup remove guildowner [user]`                                           |Removes a guild owner                                                                                     |
|`guildowners`                                                              |Lists all the owners of the server                                                                        |
|`setup add optrole [optRoleBaseName] [roleToAssignName] [requiredRoleName]`|Adds an opt role                                                                                          |
|`setup remove optrole [optRoleName]`                                       |Removes an opt role                                                                                       |
|`setup add pointrole [pointsAmount] [roleName]`                            |Automatically assigns a user a role when they reach X amount of points                                    |
|`setup remove pointrole [pointsAmount]`                                    |Removes a point role                                                                                      |
|`setup pointroles`                                                         |Displays a list of all the role points                                                                    |
|`setup set points [amount]`                                                |Sets the amount of points given                                                                           |
|`setup set pointscooldown [time]`                                          |Changes the cooldown between when points are given out                                                    |
|`setup quick`                                                              |Provides information on server quick setup                                                                |
|`setup quick rules`                                                        |Displays the template rules                                                                               |
|`setup quick start`                                                        |Provides the ability to quickly setup your server with this bot                                           |
|`setup add roleping [roleToChangeName] [roleToNotAllowToMention]`          |Adds a role to role ping                                                                                  |
|`setup remove roleping [roleToChangeName] [roleAllowedToMentionName]`      |Removes a role to role ping                                                                               |
|`rolepings`                                                                |Lists all role to role pings                                                                              |
|`setup set rulemessage [id] [silenceMessage]`                              |Sets what message that users need to react to. Run this command in the same channel as were the message is|
|`setup set rulerole [rolename]`                                            |Sets what role to give once a user successfully reacts to the rule message                                |
|`setup set ruleemoji [emoji]`                                              |Sets the emoji that users have to use to gain access                                                      |
|`setup toggle rulereaction`                                                |Enables/Disables the rule reaction feature. All the other commands MUST be ran before this one            |
|`setup set warnskick [warningsNeeded]`                                     |Sets how many warnings until a user gets kicked                                                           |
|`setup set warnsban [warningsNeeded]`                                      |Sets how many warnings until a user gets banned                                                           |
|`setup welcomechannel [channel]`                                           |Sets where the custom welcome and goodbye messages will go                                                |
|`setup toggle welcomemessage`                                              |Enables/Disables the custom welcome message                                                               |
|`setup toggle goodbyemessage`                                              |Enables/Disables the custom goodbye message                                                               |
|`setup set welcomemessage [message]`                                       |Sets the welcome message                                                                                  |
|`setup set goodbyemessage [message]`                                       |Sets the goodbye message                                                                                  |

## Bot Owner Commands

Theses commands can **ONLY** be executed by the bot owner!

### Bot Owner Commands

|Command                                   |Summary                                                                                                     |
|------------------------------------------|------------------------------------------------------------------------------------------------------------|
|`addxp [user] [amount]`                   |Adds XP to a user.                                                                                          |
|`removexp [user] [amount]`                |Removes XP from a user.                                                                                     |
|`leaveguild [guildID]`                    |Makes the bot leave a Guild.                                                                                |
|`guildlist`                               |Gets a list of all the Guilds the bot is in.                                                                |
|`commands`                                |Gets all loaded commands.                                                                                   |
|`modules`                                 |Gets all loaded modules.                                                                                    |
|`addcustomprofilemessage [user] [message]`|Adds a custom high level profile message. [Read this for more info](../../../hosting/highlevelprofilemsg)   |
|`removecustomprofilemessage [user]`       |Removes a custom high level profile message. [Read this for more info](../../../hosting/highlevelprofilemsg)|

### Bot Throw Exception

|Command                   |Summary                             |
|--------------------------|------------------------------------|
|`throwexception [message]`|Forces the bot to throw an exception|
