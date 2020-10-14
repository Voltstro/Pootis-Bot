# Anti Spam

Pootis-Bot includes some anti-spam features.

### General Commands

|Command     |Summary                                                    |
|------------|-----------------------------------------------------------|
|`setup spam`|Shows setup info regarding the server's anti-spam settings.|

## Mass Ping Anti-Spam

Do you have bots/users/trolls coming onto your Discord server and pinging everyone's name in one long message?

Well Pootis-Bot can counter that! If a user pings more then `x`% amount of users on the Discord server in a single message then the message will be deleted.

This is mainly a problem with bigger servers but anyway its still a good idea to have this enabled.

### Commands

|Command                                    |Summary                                                                                        |Default|
|-------------------------------------------|-----------------------------------------------------------------------------------------------|-------|
|`spam toggle mentionuserspam`              |Enables / Disables the mention user anti-spam feature                                          |enabled|
|`spam set mentionuserthreshold [threshold]`|Set how much of a percentage of a servers users need to be mention before it is considered spam|65     |

## Role to Role Ping

Don't want a user with a certain role to be able to ping someone else with a role?

Once again Pootis-Bot can also counter that. If *John Smith* that has the *Member* role tries to ping *Adams* that has the role *President*, their message will be deleted. If *John Smith* continues to do so, he will receive a notification telling them to stop, and that he has received a warning.

This is good if you have a role that you may only want admins to ping.

### Commands

|Command                                                              |Summary                                                                              |
|---------------------------------------------------------------------|-------------------------------------------------------------------------------------|
|`setup add roleping [RoleToChangeName] [RoleToNotAllowToMention]`    |Stops `[RoleToChangeName]` to ping anyone with the role `[RoleToNotAllowToMention]`  |
|`setup remove roleping [RoleToChangeName] [RoleAllowedToMentionName]`|Allows `[RoleToChangeName]` to ping anyone with the role `[RoleAllowedToMentionName]`|
|`spam set roletorolewarnings [warnings]`                             |Sets how many role to role mention warnings before a proper warning will be given out|