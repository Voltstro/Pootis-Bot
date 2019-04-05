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

            await dm.SendMessageAsync("Here is a list of the current commands I can do ! :relieved: (Please note that some commands may note work depending on server settings and your role :dark_sunglasses: ) \n<:GitHub:529571722991763456> Vist <https://creepysin.github.io/Pootis-Bot/commands/discord-commands/> for more info.\n\n");
            
            List<string> parts = new List<string>();

            foreach(var moduel in _service.Modules) //Get a list of all the modules
            {
                StringBuilder cmd = new StringBuilder();
                cmd.Append("```diff\n+ " + moduel.Name);

                var commands = moduel.Commands;
                foreach(var command in commands)
                {
                    cmd.Append($"\n- {command.Name} \n  └ Summary: {command.Summary}\n  └ Alias: {FormatAliases(command)}\n  └ Usage: `{command.Name} {FormatParms(command)}`");
                }

                cmd.Append("\n```");

                parts.Add(cmd.ToString());
            }

            int currentmod = 0;
            int maxmod = parts.Count();
            var desarray = parts.ToArray();

            while (currentmod != maxmod) //Go through all moduels
            {
                //string item = "";
                StringBuilder mod = new StringBuilder();

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
                                    mod.Append(desarray[currentmod]);
                                    currentmod += 1;
                                }
                                else
                                {
                                    count += desarray[currentmod].Count() + desarray[currentmod + 1].Count();
                                    mod.Append(desarray[currentmod] + desarray[currentmod + 1]);
                                    currentmod += 2;
                                }
                            }
                            else
                            {
                                mod.Append( desarray[currentmod]);
                                currentmod += 1;
                                count = 1400;
                            }
                        }
                    }
                    catch (IndexOutOfRangeException) //Last module
                    {
                        mod.Append(desarray[currentmod]);
                        currentmod = maxmod;
                        count = 1400;
                    }
                }

                await dm.SendMessageAsync(mod.ToString());
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

            StringBuilder format = new StringBuilder();
            
            int count = aliases.Count;
            int currentCount = 1;
            foreach(var alias in aliases)
            {      
                format.Append(alias);

                if (currentCount != count)
                {
                    format.Append(", ");
                }
                currentCount += 1;
            }

            return format.ToString();
        }

        private string FormatParms(CommandInfo commandinfo)
        {
            var parms = commandinfo.Parameters;

            StringBuilder format = new StringBuilder();
            int count = parms.Count;
            if (count != 0) format.Append("[");
            int currentCount = 1;
            foreach (var parm in parms)
            {
                format.Append(parm);

                if (currentCount != count) format.Append(", ");
                currentCount += 1;
            }

            if (count != 0) format.Append("]");

            return format.ToString();
        }
    }
}
