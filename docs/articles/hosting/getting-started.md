# Getting Started

## Prerequisites

- Docker
- Postgres (tested against Postgres 17)

## Database Setup

Pootis-Bot requires a Postgres database. The app will handle migrating the database, but as such, will require the user who connects to the database to have permissions to create objects in the database.

The connection string used should end up looking like this:

```
Host=<Database Host>;Username=pootis;Password=<Password>;Database=pootis
```

You can find more details on options for the connection string in [Npgsql docs](https://www.npgsql.org/doc/connection-string-parameters.html).

## Apps

You can find the Docker image on [Docker Hub](https://hub.docker.com/r/voltstro/pootis-bot).

TODO: Write docker run, and docker compose example

## Discord Application (Bot) Setup

You will need to configure a Discord application and get its bot's token so that Pootis-Bot can interactive with Discord. You will also need to add the bot to your Discord server. If you haven't done that yet (or don't know how to), please read the [token setup article](token.md).

## Config

Base global configuration of the bot is done by either setting the options in `appsettings.json`, or by setting the options using environment variables.

Setting options by the environment options is done in a format of `<Key>__<Key>`.

At a bare minimal, the `Config.BotToken` needs to be set to your Discord bot's token, and the `ConnectionString.PootisConnection` options set to your database connection string.

Example settings for the bot:

```ini
ConnectionString__PootisConnection="Host=<Database Host>;Username=pootis;Password=<Password>;Database=pootis"
Config__BotToken="DiscordBotToken"
Config__BotName="Pootis"
Config__EnableAudioServices=false
```
