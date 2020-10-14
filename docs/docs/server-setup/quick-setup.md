# Quick Setup

Quick setup allows the bot to quickly setup a lot of the features that bot has on your Discord server for you.

!!! note
    Please note that this feature is designed for new servers!

## What does it do?

1. This first thing the bot does is setup a new role called 'Member'. (If a role with the same name exist, it will be overwritten!) 

    - This role will have the permissions: Create Instant Invite, View Channels, Send Messages, Embed Links, Attach Files, Read Message History, Add Reactions, Connect and Speak

2. It will setup a new channel welcome channel (or use a pre-existing one, if it does exist), and set up [welcome and goodbye messages](../welcomemessage/).

3. Setup a new channel called '#rules'. It will then put the template rules message in the channel, and setup [rule reaction](../rulereaction/) for it, using the '👌' emoji.

    - The channel will have @everyone set to deny sending messages to the channel, and enable viewing the channel and message history

4. Add two categories called 'General' and 'Gaming', both with a text channel, and an [auto VC](../auto-vc/) channel.

    - Theses channels will allow members to talk, and do normal 'member things'. But the @everyone role won't be able to.

