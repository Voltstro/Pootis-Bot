using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules;

public class BasicModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string GitHubLink = "https://github.com/Voltstro/Pootis-Bot";
    private const string DocLink = "https://projects.voltstro.dev/Pootis-Bot/latest/";
    private const string DiscordLink = "https://discord.voltstro.dev";

    private readonly PootisBotConfig config;
    
    public BasicModule(IOptions<PootisBotConfig> config)
    {
        this.config = config.Value;
    }
    
    [SlashCommand("hello", "Provides about info")]
    public async Task Hello()
    {
        EmbedBuilder embed = new();
        embed.WithTitle("Hello!");
        embed.WithDescription($"Hello! My name is {config.BotName}!\n\n**__Links__**" +
                              $"\n<:GitHub:529571722991763456> [Github Page]({GitHubLink})" +
                              $"\n:bookmark: [Documentation]({DocLink})" +
                              $"\n<:Discord:529572497130127360> [Voltstro Discord Server]({DiscordLink})" +
                              $"\n\nThis project is under the [MIT license]({GitHubLink}/blob/master/LICENSE.md)");
        //embed.WithFooter(
        //    $"Pootis-Bot: v{VersionUtils.GetApplicationVersion()} - Discord.Net: v{VersionUtils.GetDiscordNetVersion()}");
        embed.WithColor(new Color(241, 196, 15));
        await RespondAsync(embed: embed.Build());
    }
    
    [SlashCommand("ping", "Gets the ping of the bot")]
    public async Task Ping()
    {
        await RespondAsync($"Ping Pong! {Context.Client.Latency}ms.");
    }
    
    [SlashCommand("roll", "Roles a number")]
    public async Task Roll(int min = 0, int max = 6)
    {
        if (min >= max)
        {
            await RespondAsync("The min value cannot be the same or larger as the max value!");
            return;
        }

        await RespondAsync($"I rolled a **{new Random().Next(min, max)}**!");
    }

    [SlashCommand("server", "Displays information about the server")]
    [CommandContextType(InteractionContextType.Guild)]
    public async Task Server()
    {
        SocketGuild guild = Context.Guild;

        EmbedBuilder embed = new();
        embed.WithTitle("Server Details");
        embed.WithDescription("**__Server__**" +
                              $"\n**Server Name:** {guild.Name}" +
                              $"\n**Server Id:** {guild.Id}" +
                              $"\n**Server Member Count:** {guild.MemberCount}" +
                              "\n\n**__Server Owner__**" +
                              $"\n**Owner Name: **{guild.Owner.Username}");
        embed.WithThumbnailUrl(guild.IconUrl);
        embed.WithColor(new Color(241, 196, 15));
        await RespondAsync(embed: embed.Build());
    }
}