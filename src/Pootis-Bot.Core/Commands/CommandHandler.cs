using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Commands.Permissions;
using Pootis_Bot.Core;
using Pootis_Bot.Discord.TypeConverters;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;
using Emoji = Pootis_Bot.Discord.Emoji;
using IResult = Discord.Interactions.IResult;

namespace Pootis_Bot.Commands;

/// <summary>
///     Handles commands for Discord
/// </summary>
internal sealed class CommandHandler
{
    private readonly DiscordSocketClient client;
    
    private readonly InteractionService interactionService;

    private readonly List<IPermissionProvider> permissionProviders;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    ///     Creates a new <see cref="CommandHandler" /> instance
    /// </summary>
    /// <param name="client"></param>
    internal CommandHandler(DiscordSocketClient client)
    {
        this.client = client;
        client.InteractionCreated += HandleInteraction;
        client.MessageReceived += HandleMessage;

        interactionService = new InteractionService(client);
        interactionService.AddTypeConverter<Emoji>(new EmojiTypeConverter());
        
        interactionService.SlashCommandExecuted += CommandExecuted;

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddSingleton(client)
            .AddSingleton(interactionService);
        ModuleManager.InstallServicesFromLoadedModules(serviceCollection);
        serviceProvider = serviceCollection.BuildServiceProvider();
        
        permissionProviders = new List<IPermissionProvider>();
    }

    /// <summary>
    ///     Install modules in an assembly
    /// </summary>
    /// <param name="assembly"></param>
    internal void InstallAssemblyModules(Assembly assembly)
    {
        interactionService.AddModulesAsync(assembly, serviceProvider);
    }

    /// <summary>
    ///     Adds a <see cref="IPermissionProvider" />
    /// </summary>
    /// <param name="permissionProvider"></param>
    internal void AddPermissionProvider(IPermissionProvider permissionProvider)
    {
        permissionProviders.Add(permissionProvider);
    }

    /// <summary>
    ///     Registers all interaction commands to all guilds
    /// </summary>
    internal async Task RegisterInteractionCommands()
    {
#if DEBUG
        ulong? testingGuidId = BotConfig.Instance.TestingGuildId;
        if (testingGuidId.HasValue && testingGuidId.Value != 0)
            await interactionService.RegisterCommandsToGuildAsync(testingGuidId.Value);
        else
#endif
            await interactionService.RegisterCommandsGloballyAsync(true);
    }

    #region Interaction Commands

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            SocketInteractionContext ctx = new(client, interaction);

            switch (interaction)
            {
                case ISlashCommandInteraction slashCommandInteraction:
                    SearchResult<SlashCommandInfo> result =
                        interactionService.SearchSlashCommand(slashCommandInteraction);
                    if (CheckSearchResult(result))
                        ExecuteSlashCommand(ctx, result.Command);
                    break;
            }

            //await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error handling interaction!");

            //If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            //response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type == InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    private async void ExecuteSlashCommand(SocketInteractionContext ctx, SlashCommandInfo slashCommandInfo)
    {
        //Check permissions
        if (ctx.User.Id != ctx.Guild.Owner.Id) //Guild owners always have everything
            if (ctx.User is SocketGuildUser {GuildPermissions.Administrator: false})
            {
                //Check permissions with the command
                CommandPermissionResult permissionResult =
                    await CheckSlashCommandWithPermissionProviders(slashCommandInfo, ctx);
                if (!permissionResult.IsSuccess)
                {
                    await ctx.Interaction.RespondAsync(permissionResult.ErrorReason);
                    return;
                }
            }

        await slashCommandInfo.ExecuteAsync(ctx, serviceProvider);
    }
    
    private Task CommandExecuted(SlashCommandInfo command, IInteractionContext ctx, IResult result)
    {
        Logger.Debug(CheckInteractionResult(ctx, result)
            ? $"Success handling slash command {command.Name}"
            : $"Error handling slash command {command.Name}. If their is an issue you need to worry about it would have already been displayed.");
        
        return Task.CompletedTask;
    }

    private bool CheckInteractionResult(IInteractionContext ctx, IResult result)
    {
        if (result.IsSuccess)
            return true;

        switch (result.Error)
        {
            case InteractionCommandError.UnknownCommand: //Interactions shouldn't ever have this right?
                ctx.Interaction.RespondAsync("Unknown Command!");
                break;
            case InteractionCommandError.ParseFailed:
            case InteractionCommandError.ConvertFailed:
            case InteractionCommandError.BadArgs:
                ctx.Interaction.RespondAsync($"Command has bad arguments! {result.ErrorReason}");
                break;
            case InteractionCommandError.Exception:
            case InteractionCommandError.Unsuccessful:
                ctx.Interaction.RespondAsync(
                    "Sorry, but an internal error occured while executing this command!");
                Logger.Error($"An error occured while executing a command!\n{result.ErrorReason}");
                break;
            case InteractionCommandError.UnmetPrecondition:
                ctx.Interaction.RespondAsync(
                    "Sorry, but you don't meet the preconditions to run this command!");
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    private async Task<CommandPermissionResult> CheckSlashCommandWithPermissionProviders(SlashCommandInfo commandInfo,
        SocketInteractionContext context)
    {
        foreach (IPermissionProvider permissionProvider in permissionProviders)
            //Try/Catch this since its most likely talking with third-party module code
            try
            {
                PermissionResult result = await permissionProvider.OnExecuteSlashCommand(commandInfo, context);
                if (!result.IsSuccess)
                    return CommandPermissionResult.FromError(result.ErrorReason);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An internal error occured with the permission provider {PermissionProviderName}!",
                    permissionProvider.GetType().Name);
                return CommandPermissionResult.FromError(
                    "An internal error occured while checking the command's permissions!");
            }

        return CommandPermissionResult.FromSuccess();
    }

    private static bool CheckSearchResult<T>(SearchResult<T> searchResult)
        where T : class, ICommandInfo
    {
        return searchResult.IsSuccess;
    }

    #endregion
    
    private Task HandleMessage(SocketMessage message)
    {
        if(message is SocketUserMessage userMessage)
            ModuleManager.ModulesClientMessage(client, userMessage);
        
        return Task.CompletedTask;
    }
}