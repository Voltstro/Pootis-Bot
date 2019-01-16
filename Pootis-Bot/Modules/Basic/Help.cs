using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules.Basic
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService commandService)
        {
            _service = commandService;
        }

        [Command("help")]
        [Summary("Gets help")]
        public async Task Help()
        {
            var dm = await Context.User.GetOrCreateDMChannelAsync();

            await Context.Channel.SendMessageAsync("I sent the help info to your dms!");

            await dm.SendMessageAsync("```css\nHere is a list of the current commands I can do !```\n");
            

            List<string> parts = new List<string>();

            foreach(var moduel in _service.Modules) //Get a list of all the modules
            {
                string data = "";
                data += "```diff\n+ " + moduel.Name;

                var commands = moduel.Commands;
                foreach(var command in commands)
                {
                    data += $"\n- {command.Name} \n  └ Summary: {command.Summary}\n  └ Alias: {FormatAliases(command)}\n";
                }

                data += "\n```";

                parts.Add(data);
            }

            int currentmod = 0;
            int maxmod = parts.Count();
            var desarray = parts.ToArray();
            

            while(currentmod != maxmod - 1)
            {    
                string item = "";
                item += desarray[currentmod];
                currentmod += 1;
                item += desarray[currentmod];
                currentmod += 1;
                item += desarray[currentmod];
                currentmod += 1;

                await dm.SendMessageAsync(item);     
            }

            await dm.SendMessageAsync("\nVist <https://creepysin.github.io/docs/Pootis-Bot/commands> for more info.\n\n");
        }

        private string FormatAliases(CommandInfo commandinfo)
        {
            var aliases = commandinfo.Aliases;

            string format = "";
            int count = aliases.Count;
            int currentCount = 1;
            foreach(var alias in aliases)
            {      
                format += alias;

                if (currentCount != count) format += ", ";
                currentCount += 1;
            }

            return format;
        }
    }
}
