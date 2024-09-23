# Welcome & Goodbye Message

Pootis-Bot has a custom welcome and goodbye message system.

The welcome message is a message that is sent everytime a new user joins the server.

To set the welcome message use the command:

```bash
setup set welcomemessage [message]
```

You would set the `[message]` to be your message you want to greet new users with.

For example:
```bash
setup set welcomemessage Hello and welcome!
```

You can also use `[user]` and `[server]` in the message to mention the user and put in the current server name.

For example:
```bash
setup set welcomemessage Hello [user]! Thanks for joining [server]! Please check out the rules first then enjoy your stay
```

This would produce something that looks like this when a user joins the server.

![Welcome Message Example](../assets/images/WelcomeMessage/1-WelcomeMessage.jpg)

## Setting goodbye message

The goodbye message is basically exactly the same as the welcome message, except it is sent when a user leaves the server.

To set the goodbye message use the command:
```bash
setup set goodbyemessage [message]
```

For example:
```bash
setup set goodbyemessage Goodbye!
```

Like welcome messages there is `[user]` but there is **NO** `[server]`.

For example:
```bash
setup set goodbyemessage Goodbye [user]. We hope you enjoyed your stay.
```

This would produce something that looks like this when a user leaves the server.

![Welcome Message Example](../assets/images/WelcomeMessage/2-GoodbyeMessage.jpg)

## Setting the channel

Once you have your messages set, you can now set the channel that they will be sent to! Execute this command with the name of channel you want the messages to be sent to.
```bash
setup welcomechannel [channel]
```

Example:
```bash
setup welcomechannel welcome
```

## Enabling / Disabling

Once you have your messages set, as well as the channel for the messages to be sent to, you can run this command to enable / disable welcome messages.
```bash
setup toggle welcomemessage
```

You can do this command to enable or disable goodbye messages.
```bash
setup toggle goodbyemessag
```
