using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Managers;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Services
{
	public class PermissionService
	{
		private readonly string[] _blockedCmds = {"profile", "profilemsg", "hello", "ping", "perm"};
		private readonly CommandService _service;

		public PermissionService(CommandService commandService)
		{
			_service = commandService;
		}

		/// <summary>
		/// Lets only a certain role use a command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="role"></param>
		/// <param name="channel"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task AddPerm(string command, string role, IMessageChannel channel, SocketGuild guild)
		{
			if (!CanModifyPerm(command))
			{
				await channel.SendMessageAsync($"Cannot set the permission of **{command}**");
				return;
			}

			if (!DoesCmdExist(command))
			{
				await channel.SendMessageAsync($"The command **{command}** doesn't exist!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(guild);

			//Check too see if role exist
			if (Global.GetGuildRole(guild, role) != null)
			{
				if (server.GetCommandInfo(command) == null) // Command doesn't exist, add it
				{
					ServerList.CommandInfo item = new ServerList.CommandInfo
					{
						Command = command,
						Roles = new List<string> { role }
					};

					server.CommandInfos.Add(item);

					await channel.SendMessageAsync($"The role **{role}** will be allowed to use the command **{command}**.");
				}
				else // The command already exist, add it to the list of roles.
				{
					server.GetCommandInfo(command).Roles.Add(role);

					await channel.SendMessageAsync($"The role **{role}** will be allowed to use the command **{command}**.");
				}

				ServerListsManager.SaveServerList();
			}
			else
			{
				await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
			}
		}

		/// <summary>
		/// Stops a role from being able to use a command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="role"></param>
		/// <param name="channel"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task RemovePerm(string command, string role, IMessageChannel channel, SocketGuild guild)
		{
			if (!CanModifyPerm(command))
			{
				await channel.SendMessageAsync($"Cannot set the permission of **{command}**");
				return;
			}

			if (!DoesCmdExist(command))
			{
				await channel.SendMessageAsync($"The command **{command}** doesn't exist!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(guild);

			//Check too see if role exist
			if (Global.GetGuildRole(guild, role) != null)
			{
				if (server.GetCommandInfo(command) == null) // Command already has no permissions
				{
					await channel.SendMessageAsync(
						$"The command **{command}** already has no permissions added to it!");
					return;
				}

				bool roleRemoved = false;
				foreach (string cmdRole in server.GetCommandInfo(command).Roles.ToArray())
				{
					if (roleRemoved)
						continue;

					if (role != cmdRole) continue;
					server.GetCommandInfo(command).Roles.Remove(role);
					roleRemoved = true;

					await channel.SendMessageAsync($"The role **{role}** was removed from the command **{command}**.");
				}

				if (!roleRemoved)
					await channel.SendMessageAsync($"The command **{command}** didn't had the role **{role}** on it!");

				if (server.GetCommandInfo(command).Roles.Count == 0)
					server.CommandInfos.Remove(server.GetCommandInfo(command));

				ServerListsManager.SaveServerList();
			}
			else
			{
				await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
			}
		}

		private bool CanModifyPerm(string command)
		{
			bool canModifyPerm = true;
			foreach (string cmd in _blockedCmds)
				if (command == cmd)
					canModifyPerm = false;

			return canModifyPerm;
		}

		private bool DoesCmdExist(string command)
		{
			// ReSharper disable once NotAccessedVariable
			bool doesCmdExist = false;

			foreach (ModuleInfo module in _service.Modules) //Get the command info
			{
				if (doesCmdExist)
					continue;

				foreach (CommandInfo commandInfo in module.Commands)
					if (commandInfo.Name == command)
					{
						doesCmdExist = true;
					}
			}

			return doesCmdExist;
		}
	}
}