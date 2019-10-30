using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		/// <param name="roles"></param>
		/// <param name="channel"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task AddPerm(string command, string[] roles, IMessageChannel channel, SocketGuild guild)
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

			//Check all roles to see if they exists
			foreach (string role in roles)
			{
				if (Global.GetGuildRole(guild, role) != null) continue;

				await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
				return;
			}

			if (server.GetCommandInfo(command) == null)
			{
				//The command didn't exist before, so we will create a new one and just add the roles
				server.CommandInfos.Add(new ServerList.CommandInfo
				{
					Command = command,
					Roles = roles.ToList()
				});

				ServerListsManager.SaveServerList();

				await channel.SendMessageAsync(AddPermMessage(roles, command));
			}
			else //The command already exists
			{
				//Build a list of command
				foreach (string role in roles)
				{
					if (server.GetCommandInfo(command).GetRole(role) != null)
					{
						await channel.SendMessageAsync($"The command `{command}` already has the role **{role}** added to it!");
						return;
					}

					server.GetCommandInfo(command).Roles.Add(role);
				}

				ServerListsManager.SaveServerList();
				await channel.SendMessageAsync(AddPermMessage(roles, command));
			}
		}

		/// <summary>
		/// Stops a role from being able to use a command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="roles"></param>
		/// <param name="channel"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public async Task RemovePerm(string command, string[] roles, IMessageChannel channel, SocketGuild guild)
		{
			if (!CanModifyPerm(command))
			{
				await channel.SendMessageAsync($"Cannot set the permission of the command `{command}`.");
				return;
			}

			if (!DoesCmdExist(command))
			{
				await channel.SendMessageAsync($"The command `{command}` doesn't exist!");
				return;
			}

			ServerList server = ServerListsManager.GetServer(guild);

			//Check all the imputed roles to see if they exists
			foreach (string role in roles)
			{
				if (Global.GetGuildRole(guild, role) != null) continue;

				await channel.SendMessageAsync($"The role **{role}** doesn't exist!");
				return;
			}

			//Doesn't exist
			if (server.GetCommandInfo(command) == null)
			{
				await channel.SendMessageAsync($"The command `{command}` already has no roles assigned to it!");
			}
			else 
			{
				//Check all roles and make sure they are assigned to the command
				foreach (string role in roles)
				{
					if (server.GetCommandInfo(command).GetRole(role) != null) continue;

					await channel.SendMessageAsync(
						$"The command `{command}` doesn't have the role **{role}** assigned to it!");
					return;
				}

				//Now begin removing all the roles, since we know all the entered roles are already assigned to the command
				foreach (string role in roles)
				{
					server.GetCommandInfo(command).Roles.Remove(server.GetCommandInfo(command).GetRole(role));
				}

				//There are no more roles assigned to the command so remove it entirely
				if (server.GetCommandInfo(command).Roles.Count == 0)
				{
					server.CommandInfos.Remove(server.GetCommandInfo(command));
				}

				ServerListsManager.SaveServerList();
				await channel.SendMessageAsync(RemovePermMessage(roles, command, server));
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

		private static string AddPermMessage(IReadOnlyList<string> roles, string command)
		{
			//There is only one role
			if (roles.Count == 1)
			{
				return $"**{roles[0]}** role will be allowed to use the command `{command}`.";
			}
			else //Multiple roles
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < roles.Count; i++)
				{
					sb.Append(i == roles.Count - 1 ? roles[i] : $"{roles[i]}, ");
				}

				return $"**{sb}** roles will be allowed to use the command `{command}`.";
			}
		}

		private static string RemovePermMessage(IReadOnlyList<string> roles, string command, ServerList server)
		{
			if (server.GetCommandInfo(command) == null)
			{
				return $"Anyone will now be allowed to use the command `{command}`.";
			}

			//There is only one role
			if (roles.Count == 1)
			{
				return $"**{roles[0]}** role will not be allowed to use the command `{command}`.";
			}
			else //Multiple roles
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < roles.Count; i++)
				{
					sb.Append(i == roles.Count - 1 ? roles[i] : $"{roles[i]}, ");
				}

				return $"**{sb}** roles will not be allowed to use the command `{command}`.";
			}
		}
	}
}