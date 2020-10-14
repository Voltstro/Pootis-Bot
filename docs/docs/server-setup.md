Everything in the Server Setup section should help you get Pootis-Bot working for your server!

## Setting up Pootis-Bot for your server

!!! note
    All commands that are contained in the server setup section require to be run by the Discord server's owner!

    If you want other users to be able to use owner level commands, follow [this guide](adding-owners/).

At anytime you can run the command `setup status` to get detailed info about your server setup status.

There is also a feature that allows you to quickly setup the bot with all of its features. It is designed for new servers. You can read more about that in the [Quick Setup](quick-setup/) section.

### Important commands

These commands are all quite powerful and should have a [permission added onto them](perm-system/). **NOTE**: Theses commands can be accessed by anyone, assuming they meet the requirements for it: E.G: for kick you would need the ability to kick users.

|Command          |Summary                                                                                                                      |
|-----------------|-----------------------------------------------------------------------------------------------------------------------------|
|`warn`           |Warns a user. When the warning count gets to three they are kicked from the server, and when it reaches four they are baned. |
|`makewarnable`   |Allows a user to be made warn-able.                                                                                          |
|`makenotwarnable`|Allows a user to be not warn-able.                                                                                           |
|`kick`           |Kicks a user.                                                                                                                |
|`ban`            |Bans a user.                                                                                                                 |
|`addvcchannel`   |Adds a [auto-vc channel](auto-vc/).                                                                                          |

## Useful features

These features are quite useful but are not required.

- [Rule Reaction System](rulereaction/) | Makes a newly joined user have to react to a message before allowed access to the rest of the server. Can help stop spam, and spam bots.
- [Welcome & Goodbye Messages](welcomemessage/) | Allows the use of a custom welcome message and goodbye message for when a user joins/leaves.
- [Auto Voice Channels](auto-vc/) | Can make your Discord server's voice channels look at lot cleaner.
- [Anti Spam Features](anti-spam/) | Provides features to help stop spamming.