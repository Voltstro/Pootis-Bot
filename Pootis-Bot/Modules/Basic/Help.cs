using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pootis_Bot.Modules.Basic
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        // Module Infomation
        // Orginal Author   - Creepysin
        // Description      - The two help commands
        // Contributors     - Creepysin, 

        private readonly CommandService cmdService;

        public Help(CommandService commandService)
        {
            cmdService = commandService;
        }

        [Command("help")]
        [Alias("h")]
        [Summary("Gets help")]
        public async Task HelpCmd()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"```# Pootis-Bot Normal Commands```\nFor more help on a specific command do `{Global.botPrefix}help [command]`.\n");

            //Basic Commands
            foreach(var helpModule in Config.bot.helpModules)
            {
                builder.Append($"\n**{helpModule.group}** - ");
                foreach(var module in helpModule.modules)
                {
                    foreach(var cmd in GetModule(module).Commands)
                    {
                        builder.Append($"`{cmd.Name}` ");
                    }
                }
            }

            await Context.Channel.SendMessageAsync(builder.ToString());
        }

        [Command("help")]
        [Alias("h", "command", "chelp", "ch")]
        [Summary("Gets help on a specific command")]
        public async Task HelpSpecific([Remainder] string query)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($"Help for {query}");
            embed.WithColor(new Color(241, 196, 15));

            var result = cmdService.Search(Context, query);
            if (result.IsSuccess)
            {
                foreach (var command in result.Commands)
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
            foreach (var alias in aliases)
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

        private ModuleInfo GetModule(string moduleName)
        {
            var result = from a in cmdService.Modules
                         where a.Name == moduleName
                         select a;

            var module = result.FirstOrDefault();
            return module;
        }
    }
}
