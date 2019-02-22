using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;

namespace Pootis_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("pick")]
        [Summary("Picks between two things")]
        public async Task PickOne([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string seletion = options[r.Next(0, options.Length)];
            await Context.Channel.SendMessageAsync("Choice for " + Context.Message.Author.Mention + "\nI Choose: " + seletion);
        }

        [Command("roll")]
        [Summary("Roles between 0 and 50 or between two custom numbers")]
        public async Task Roll(int min = 0, int max = 50)
        {
            Random r = new Random();
            int random = r.Next(min, max);
            await Context.Channel.SendMessageAsync("The number was: " + random);
        }

        [Command("embedmessage")]
        [Alias("embed")]
        [Summary("Displays your message in an embed message")]
        public async Task CmdEmbedMessage(string title = "", [Remainder]string msg = "")
        {
            await Context.Channel.SendMessageAsync("", false, EmbedMessage(title, msg).Build());
        }

        [Command("server")]
        [Summary("Gets details about the server you are in")]
        public async Task ServerGuild()
        {
            var guilduser = (SocketGuildUser)Context.User;            

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Server Details");
            embed.WithDescription($"**__Server__**" +
                $"\n**Server Name:** {guilduser.Guild}" +
                $"\n**Server ID:** {guilduser.Guild.Id}" +
                $"\n**Server Member Count:** {guilduser.Guild.MemberCount}" +
                $"\n\n**__Server Owner__**" +
                $"\n**Owner Name: **{guilduser.Guild.Owner.Username}" +
                $"\n**Owner ID: ** {guilduser.Guild.OwnerId}");
            embed.WithThumbnailUrl(guilduser.Guild.IconUrl);
            embed.WithColor(new Color(241, 196, 15));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("ping")]
        [Summary("Ping Pong!")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync($"Pong! {Context.Client.Latency}ms");
        }

        #region Functions

        EmbedBuilder EmbedMessage(string title, string msg)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(msg);

            return embed;
        }

        #endregion
    }
}
