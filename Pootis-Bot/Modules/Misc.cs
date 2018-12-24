using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.UserAccounts;

namespace Pootis_Bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("pick")]
        public async Task PickOne([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string seletion = options[r.Next(0, options.Length)];
            await Context.Channel.SendMessageAsync("Choice for " + Context.Message.Author.Mention + "\nI Choose: " + seletion);
        }

        [Command("creepysin")]
        public async Task Creepysin()
        {
            await Context.Channel.SendMessageAsync("Heres creepysin channel: \n https://bit.ly/2KfKeAf");
        }
    }
}
