# Contributing

First, thank you for considering contributing to Pootis-Bot. People like you can help make Pootis-Bot an even greater Discord bot.

We love to receive contributions from our community, wether it is bug reports and feature requests or writing code that can be added
into Pootis-Bot, we would love to recieve it!

However, please don't use the issue tracker for support questions. If you need support you can ask on the [Voltstro Discord Server](https://discord.voltstro.dev).
If your issue isn't exactly Pootis-Bot specific you could ask on the [Discord API Server](https://discord.gg/discord-api) under #dotnet_discord-net,
or even consider Stack Overflow or other forum sites.

Also, before contributing please read the [Code of Conduct](/CODE_OF_CONDUCT.md) first!

## Expectations

Here are some expectations, this are all pretty common expectations.

* Try and keep cross-platform compatibility. We are trying to support Windows, Linux and Mac.
* Make sure your code actually compiles, [Azure Pipelines](https://dev.azure.com/Voltstro/Pootis-Bot) will immediately pick up on this.
* For any major changes create an issue for it and discuss it with the community and get community feedback.
* Welcome newcomers and encourage diverse new contributors from all backgrounds.

## Reporting a bug

When filing an issue, make sure to answer these five questions:

1. What version of Pootis-Bot are you using?
2. What operating system and processor architecture are you using?
3. What did you do?
4. What did you expect to see?
5. What did you see instead?

## Suggesting a feature or enhancement

Pootis-Bot does have a goal of being 'an all in one Discord Bot', but we all know this won't happen, and it can't happen. There are thousands apon thousands of needs that people, well need.

But that doesn't mean that we are not open for features or enchancements!

If you have a feature or enchancement request, you are probably not alone. There is more then likely others to be out there with similar needs.
To suggest something, open an issue on the GitHub issue tracker which desribes the feature or enchacement you would like to see, why you need it, and how it should work.

Also remember to add the feature or enchancement label!

## Code style

Internally, we use [Visual Studio 2019](https://visualstudio.microsoft.com) (or [JetBrian's Rider](https://www.jetbrains.com/rider/)) 
with [JetBrain's ReSharper](https://www.jetbrains.com/resharper/) extension installed (or incase with Rider it comes with it built in). 

I highly recommond using ReSharper when coding for Pootis-Bot, there is a provided [`.DotSettings`](/Pootis-Bot.sln.DotSettings) file that has a bunch of ReSharper settings that make coding easier.

---

Ok now for some Coding style rules, in-case you don't have ReSharper (or Rider).

### Comment Style

Comments start with no space between the slashes and the actuall comment, so like this:

```csharp
//Hello World!
```

Not like this:
```csharp
// Hello World! This is incorrect!
```

The expection being `<summary>` XML comments.

### Declaring new Objects

Always use explict type, so like this:

```csharp
EmbedBuilder embed = new EmbedBuilder();
embed.WithTitle("This is correct!");
```

Not like this: (no `var`)

```csharp
var embed = new EmbedBuilder();
embed.WithTitle("This is not correct!");
```

Oh also, all structs are stored in the [Structs folder](/src/Pootis-Bot/Structs).

### Modules

Please read the [README.md in the `src/Pootis-Bot/Modules` directory](src/Pootis-Bot/Modules/README.md) for information on Discord command modules.

## Community

If you would like to chat with us, you can so on our [Discord Server](https://discord.voltstro.dev). I(Voltstro) am basically active all day
that isn't a week day.

If you want to email me directly about something you can email me: me@voltstro.dev
