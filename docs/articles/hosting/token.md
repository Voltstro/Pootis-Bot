# Token Setup

You will need a Discord bot token and the bot account associated with that token added to your Discord server. The setup of the token is done via the [Discord developers portal](https://discord.com/developers/applications).

## Application Setup

Once on the developers portal, create a new application (button top right). Give it a cool name.

![App](~/assets/articles/hosting/token/app.webp)

Once created, head to the bot settings.

![Bot Settings](~/assets/articles/hosting/token/bot-settings.webp)

You can get the bot token by pressing "Reset Token". It will ask to confirm if you want to reset the token, which is fine as this is a new application.

Once confirmed, it will give you the token, store this token somewhere secure, as it will not reveal the token again, and you will have to reset it if you do forget it.

![Token](~/assets/articles/hosting/token/token.webp)

> [!WARNING]
> Never share this token publicly.

## Adding the bot account

Head over to bot's installation settings. Under default install settings, grant the bot scope to the guild install section, as well as required permissions.

![Installation](~/assets/articles/hosting/token/installation.webp)

Once set, copy the install link and navigate to the link in a new tab. Once the prompt to add your bot opens, select add to server.

![Add](~/assets/articles/hosting/token/add.webp)

Select what server to add the bot to, confirm permissions and the bot then should be added to your server.
