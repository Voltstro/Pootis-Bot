﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;

namespace Pootis_Bot.Commands.Permissions
{
    /// <summary>
    ///     The standard interface for managing permissions with Pootis
    /// </summary>
    public interface IPermissionProvider
    {
        /// <summary>
        ///     Invoked when a command is about to be executed
        /// </summary>
        /// <param name="commandInfo">Info about the command.</param>
        /// <param name="context">Context info..</param>
        /// <returns>Return true if all is good. Otherwise return false to cancel the command execution</returns>
        public Task<PermissionResult> OnExecuteCommand(CommandInfo commandInfo, ICommandContext context);

        /// <summary>
        ///     Invoked when a slash command is about to be executed
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<PermissionResult> OnExecuteSlashCommand(SlashCommandInfo info, SocketInteractionContext context);
    }
}