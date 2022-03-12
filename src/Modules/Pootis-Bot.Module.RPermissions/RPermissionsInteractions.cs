using System.Linq;
using System.Threading.Tasks;
using Cysharp.Text;
using Discord;
using Discord.Interactions;
using Pootis_Bot.Config;
using Pootis_Bot.Module.RPermissions.Entities;

namespace Pootis_Bot.Module.RPermissions;

[Group("perm", "Commands related to permissions")]
public class RPermissionsInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RPermissionsConfig config;
    private readonly InteractionService interactionService;

    public RPermissionsInteractions(InteractionService interactionService)
    {
        this.interactionService = interactionService;
        config = Config<RPermissionsConfig>.Instance;
    }

    [SlashCommand("add", "Adds a role to a command")]
    public async Task PermSet(string command, IRole role)
    {
        CommandSearchResult result = FindSlashCommand(command);
        if (result.IsSuccess && result.SlashCommand != null)
        {
            await AddPermission(result.SlashCommand, Context.Guild, role);
            return;
        }

        await RespondAsync(result.ErrorReason);
    }

    [SlashCommand("remove", "Removes a roles from a command")]
    public async Task PermRemove(string command, IRole? role = null)
    {
        CommandSearchResult result = FindSlashCommand(command);
        if (result.IsSuccess && result.SlashCommand != null)
        {
            await RemovePermission(result.SlashCommand, Context.Guild, role);
            return;
        }

        await RespondAsync(result.ErrorReason);
    }

    [SlashCommand("get", "Gets roles of a command")]
    public async Task PermGet(string command)
    {
        CommandSearchResult result = FindSlashCommand(command);
        if (result.IsSuccess && result.SlashCommand != null)
        {
            await PermissionList(result.SlashCommand, Context.Guild);
            return;
        }

        await RespondAsync(result.ErrorReason);
    }

    private CommandSearchResult FindSlashCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return CommandSearchResult.FromError("You must input a valid command name!");

        int count = interactionService.SlashCommands.Count(x => x.Name == command);
        if (count != 1)
            return CommandSearchResult.FromError("No command was found!");

        SlashCommandInfo slashCommand = interactionService.SlashCommands.First(x => x.Name == command);
        return CommandSearchResult.FromSuccess(slashCommand);
    }

    private async Task AddPermission(SlashCommandInfo commandInfo, IGuild guild, IRole role)
    {
        RPermissionServer server = config.GetOrCreateServer(guild.Id);
        RPerm commandPermissions = server.SlashCommandGetPerm(commandInfo) ?? server.SlashCommandAddPerm(commandInfo);
        if (commandPermissions.Roles.Contains(role.Id))
        {
            await RespondAsync("That role is already added to that command!");
            return;
        }

        commandPermissions.AddRole(role.Id);
        config.Save();

        await RespondAsync("Added the role to the command.");
    }

    private async Task RemovePermission(SlashCommandInfo commandInfo, IGuild guild, IRole? role)
    {
        RPermissionServer server = config.GetOrCreateServer(guild.Id);
        RPerm? commandPermissions = server.SlashCommandGetPerm(commandInfo);
        if (commandPermissions == null)
        {
            await RespondAsync("The command doesn't have any roles!");
            return;
        }

        //If there are no roles, remove them all
        if (role == null)
        {
            server.SlashCommandPermissions.Remove(commandPermissions);
            config.Save();

            await RespondAsync("All roles were removed from that command.");
            return;
        }

        if (!commandPermissions.DoesRoleExist(role.Id))
        {
            await RespondAsync("The command already doesn't have that role!");
            return;
        }

        commandPermissions.Roles.Remove(role.Id);
        config.Save();

        await RespondAsync("The role was removed from that command.");
    }

    private async Task PermissionList(SlashCommandInfo commandInfo, IGuild guild)
    {
        RPermissionServer server = config.GetOrCreateServer(guild.Id);
        RPerm? commandPermissions = server.SlashCommandGetPerm(commandInfo);
        if (commandPermissions == null)
        {
            await RespondAsync("The command doesn't have any permissions!");
            return;
        }

        Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
        sb.Append($"**Command `{commandInfo.Name}` roles**:\n - ");
        sb.Append(ZString.Join("\n - ", commandPermissions.Roles.Select(x => guild.GetRole(x).Name)));

        await RespondAsync(sb.ToString());
        sb.Dispose();
    }
}