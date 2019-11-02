# Contributing

First, thank you for considering contributing to Pootis-Bot. People like you can help make Pootis-Bot an even greater Discord bot.

Before contributing please read the [code of conduct](/CODE_OF_CONDUCT.md) first!

We love to receive contributions from our community, wether it is bug reports and feature requests or writing code that can be added
into Pootis-Bot, we would love to recieve it!

However, please don't use the issue tracker for support questions. If you need support you can ask on the [Creepysin Discord Server](https://discord.creepysin.com).
If your issue isn't exactly Pootis-Bot specific you could ask on the [Discord API Server](https://discord.gg/discord-api) under #dotnet_discord-net,
or even consider Stack Overflow.

## Expectations

Here are some expectations, this are all pretty common expectations.

* Try and keep cross-platform compatibility. We are trying to support Windows, Linux and Mac.
* Make sure your code actually compiles, Azure Pipelines will immediately pick up on this.
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

Pootis-Bot does have a goal of being 'an all in one Discord Bot', but we all know this won't happen, and it can't happen.
But that doesn't mean that we are not open for features or enchancements.

If you have a feature or enchancement request, you are probably not alone. There is more then likely others to be out there with similar needs.
To suggest something, open an issue on the GitHub tracker which desribes the feature you would like to see, why you need it, and how it 
should work.

## Code style

Internally, we use [Visual Studio 2019](https://visualstudio.microsoft.com)(or [JetBrian's Rider](https://www.jetbrains.com/rider/)) 
with [JetBrain's Resharper](https://www.jetbrains.com/resharper/) installed.

I highly recommond using Resharper will coding for Pootis-Bot, if you can affort it, or if you are a student you might be able to get the
student pack. (Like me :))

The naming of variables are just the default ReSharper style.

Generally comments are allways like this:

```csharp
//Hello!
```

Not like this:

```csharp
// Hello!
```

And declaring new objects use explict type:

```csharp
EmbedBuilder embed = new EmbedBuilder();
```

Not:

```csharp
var embed = new EmbedBuilder();
```

Oh also, all structs are stored in the [Structs folder](/Pootis-Bot/Structs).

## Community

If you would like to chat with us, you can so on our [Discord Server](https://discord.creepysin.com). I(Creepysin) am basically active all day
that isn't a week day.

If you want to email me directly about something you can email me: me@creepysin.com
