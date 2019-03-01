using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules.Basic
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - The two help commands
        // Contributors     - Creepysin, 

        private readonly CommandService _service;

        public HelpModule(CommandService commandService)
        {
            _service = commandService;
        }

        [Command("help")]
        [Alias("h")]
        [Summary("Gets help")]
        public async Task Help()
        {
            var dm = await Context.User.GetOrCreateDMChannelAsync();
            await Context.Channel.SendMessageAsync("I sent the help info to your dms!");

            await dm.SendMessageAsync("Here is a list of the current commands I can do ! :relieved: (Please note that some commands may note work depending on server settings and your role :dark_sunglasses: ) \n<:GitHub:529571722991763456> Vist <https://creepysin.github.io/Pootis-Bot/commands/> for more info.\n\n");
            
            List<string> parts = new List<string>();

            foreach(var moduel in _service.Modules) //Get a list of all the modules
            {
                string data = "";
                data += "```diff\n+ " + moduel.Name;

                var commands = moduel.Commands;
                foreach(var command in commands)
                {
                    data += $"\n- {command.Name} \n  └ Summary: {command.Summary}\n  └ Alias: {FormatAliases(command)}\n  └ Usage: `{command.Name} {FormatParms(command)}`";
                }

                data += "\n```";

                parts.Add(data);
            }

            int currentmod = 0;
            int maxmod = parts.Count();
            var desarray = parts.ToArray();

            while (currentmod != maxmod) //Go through all moduels
            {
                string item = "";

                if (desarray[currentmod].Count() < 1400)
                {
                    int count = 0;
                    try
                    {
                        while (count < 1400)
                        {
                            if (desarray[currentmod].Count() + desarray[currentmod + 1].Count() < 1400)
                            {
                                if (currentmod >= maxmod)
                                {
                                    count = 1400;
                                    item += desarray[currentmod];
                                    currentmod += 1;
                                }
                                else
                                {
                                    count += desarray[currentmod].Count() + desarray[currentmod + 1].Count();
                                    item += desarray[currentmod] + desarray[currentmod + 1];
                                    currentmod += 2;
                                }
                            }
                            else
                            {
                                item += desarray[currentmod];
                                currentmod += 1;
                                count = 1400;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException) //Last module
                    {
                        item += desarray[currentmod];
                        currentmod = maxmod;
                        count = 1400;
                    }
                }

                await dm.SendMessageAsync(item);
            }
        }

        [Command("help")]
        [Alias("h", "command", "chelp", "ch")]
        [Summary("Gets help on a specific command")]
        public async Task HelpSpecific([Remainder] string query)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Help for {query}");
            embed.WithColor(new Color(241, 196, 15));

            var result = _service.Search(Context, query);
            if(result.IsSuccess)
            {
                foreach(var command in result.Commands)
                {
                    embed.AddField(command.Command.Name, $"Summary: {command.Command.Summary}\nAlias: {FormatAliases(command.Command)}\nUsage: `{command.Command.Name} {FormatParms(command.Command)}`");
                }
            }
            if (embed.Fields.Count == 0)
                embed.WithDescription("Nothing was found for " + query);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
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

        private string FormatParms(CommandInfo commandinfo)
        {
            var parms = commandinfo.Parameters;

            string format = "";
            int count = parms.Count;
            if (count != 0) format += "[";
            int currentCount = 1;
            foreach (var parm in parms)
            {
                format += parm;

                if (currentCount != count) format += ", ";
                currentCount += 1;
            }

            if (count != 0) format += "]";

            return format;
        }
    }
}
