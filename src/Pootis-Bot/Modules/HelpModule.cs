using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pootis_Bot.Shared.Helper;

namespace Pootis_Bot.Modules;

/// <summary>
///     Module that provides the help command
/// </summary>
public class HelpModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string HelpMenuKey = "HelpMenu";
    private static readonly Type[] AllowSlashCommandParameterTypes =
    [
        typeof(string),
        typeof(int),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(bool)
    ];

    private readonly ILogger<HelpModule> logger;
    private readonly IMemoryCache memoryCache;
    private readonly InteractionService interactionService;
    
    public HelpModule(ILogger<HelpModule> logger, IMemoryCache memoryCache, InteractionService cmdService)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        interactionService = cmdService;
    }

    [SlashCommand("help", "Gets help on all commands")]
    public async Task Help()
    {
        string[]? messagesToSend;
        try
        {
            messagesToSend = memoryCache.GetOrCreate(HelpMenuKey, entry =>
            {
                //Get help groups
                HelpCommandModule[] helpGroups = GetHelpMenu();

                //Discord has a message limit of 2000 chars
                //So split everything out into as few groups as possible
                List<string> messages =
                [
                    string.Empty
                ];

                int messageIndex = 0;
                int messageCharCount = 0;
                foreach (HelpCommandModule helpGroup in helpGroups)
                {
                    string formattedHelpGroup = FormatHelpGroup(helpGroup);
                    //Check if we will go over the char limit, if so add a new message and reset
                    if (formattedHelpGroup.Length + messageCharCount > 2000)
                    {
                        messages.Add(string.Empty);
                        messageIndex++;
                        messageCharCount = 0;
                    }

                    messages[messageIndex] += formattedHelpGroup;
                    messageCharCount += formattedHelpGroup.Length;
                }
                
                return messages.ToArray();
            });

            if (messagesToSend == null)
                throw new NullReferenceException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while getting help menu!");
            await RespondAsync(Messages.CreatedFailed("help menu"));
            return;
        }

        //Now send the messages
        IMessageChannel channel = Context.Channel;
        if (Context.Guild != null)
        {
            await RespondAsync("I will DM you the help info!");
            channel = await Context.User.CreateDMChannelAsync();
        }
        else
        {
            await RespondAsync("Here is help with Pootis-Bot's commands!");
        }

        foreach (string message in messagesToSend)
        {
            await channel.SendMessageAsync(message);
        }
    }

    private HelpCommandModule[] GetHelpMenu()
    {
        List<HelpCommandModule> helpGroups = new();
        ModuleInfo[] modules = interactionService.Modules.ToArray();
        foreach (ModuleInfo module in modules)
        {
            //Exclude this module
            if (module.Name == nameof(HelpModule))
                continue;
            
            //Exclude module with no commands
            IReadOnlyList<SlashCommandInfo> slashCommands = module.SlashCommands;
            if (slashCommands.Count < 1)
                continue;
            
            HelpCommand[] helpItems = new HelpCommand[slashCommands.Count];
            for (int i = 0; i < helpItems.Length; i++)
            {
                SlashCommandInfo slashCommand = slashCommands[i];
                HelpCommandParameter[] parameters = new HelpCommandParameter[slashCommand.Parameters.Count];
                for (int j = 0; j < parameters.Length; j++)
                {
                    SlashCommandParameterInfo slashCommandParameter = slashCommand.Parameters[j];
                    string? defaultValue = null;
                    if (slashCommandParameter.DefaultValue != null && AllowSlashCommandParameterTypes.Any(t => t == slashCommandParameter.DefaultValue.GetType()))
                        defaultValue = slashCommandParameter.DefaultValue.ToString();
                    
                    parameters[j] =
                        new HelpCommandParameter(slashCommandParameter.Name, defaultValue);
                }
                
                helpItems[i] = new HelpCommand(slashCommand.Name, slashCommand.Description, parameters);
            }

            string? slashGroupName = null;
            if(!string.IsNullOrWhiteSpace(module.SlashGroupName))
                slashGroupName = module.SlashGroupName;

            string? parentModuleName = null;
            string? parentSlashGroupName = null;
            ModuleInfo? parentModule = module.Parent;
            if (parentModule != null)
            {
                parentModuleName  = parentModule.Name;
                
                if (!string.IsNullOrWhiteSpace(parentModule.SlashGroupName))
                    parentSlashGroupName = parentModule.SlashGroupName;
            }
            
            HelpCommandModule helpCommandModule = new(module.Name, slashGroupName, parentModuleName, parentSlashGroupName, module.Description, helpItems);
            helpGroups.Add(helpCommandModule);
        }
        
        return helpGroups.ToArray();
    }

    private string FormatHelpGroup(HelpCommandModule helpCommandModule)
    {
        StringBuilder sb = new();
        sb.AppendLine("```diff");
        sb.Append("+ ");
        if (helpCommandModule.ParentModuleName != null)
            sb.Append($"{helpCommandModule.ParentModuleName} / ");
        
        sb.AppendLine(helpCommandModule.Name);
        sb.AppendLine($"  - Summary: {helpCommandModule.Description}");
        sb.AppendLine();

        foreach (HelpCommand helpCommand in helpCommandModule.Commands)
        {
            sb.Append("- ");
            if (helpCommandModule.GroupName != null)
                sb.Append($"{helpCommandModule.GroupName} ");
            sb.AppendLine(helpCommand.Name);
            sb.AppendLine($"  - Summary: {helpCommand.Description}");
            
            sb.Append("  - Usage: `/");
            if (helpCommandModule.ParentGroupName != null)
                sb.Append($"{helpCommandModule.ParentGroupName} ");
            if (helpCommandModule.GroupName != null)
                sb.Append($"{helpCommandModule.GroupName} ");
            sb.Append(helpCommand.Name);
            foreach (HelpCommandParameter parameter in helpCommand.Parameters)
            {
                sb.Append(" <");
                sb.Append(parameter.Name);
                if (parameter.DefaultValue != null)
                    sb.Append($" = {parameter.DefaultValue}");
                sb.Append(">");
            }
            
            sb.AppendLine("`");
        }
        
        sb.Append("```");
        return sb.ToString();
    }

    /// <summary>
    ///     Details of a module and its commands
    /// </summary>
    /// <param name="Name">The name of the module</param>
    /// <param name="GroupName">The name of the group</param>
    /// <param name="ParentModuleName">The name of the parent module</param>
    /// <param name="Description">Description of the module</param>
    /// <param name="Commands">Commands in the module</param>
    private record HelpCommandModule(string Name, string? GroupName, string? ParentModuleName, string? ParentGroupName, string Description, HelpCommand[] Commands);
    
    private record HelpCommand(string Name, string Description, HelpCommandParameter[] Parameters);

    private record HelpCommandParameter(string Name, string? DefaultValue);
}