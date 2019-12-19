using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Pootis_Bot.Core.Managers;

namespace Pootis_Bot.Preconditions
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class RequireGuildOwnerAttribute : PreconditionAttribute
	{
		private readonly bool _allowOtherOwners;

		public RequireGuildOwnerAttribute(bool allowOtherGuildOwners = true) =>
			_allowOtherOwners = allowOtherGuildOwners;

		/// <summary>
		/// Checks that a given user is the owner of a guild, and returns the result
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
			IServiceProvider services)
		{
			//Check if the user is the actual owner of the Guild
			if (context.User.Id == context.Guild.OwnerId)
				return Task.FromResult(PreconditionResult.FromSuccess());

			ulong userId = ServerListsManager.GetServer((SocketGuild) context.Guild).GetAGuildOwner(context.User.Id);
			if (_allowOtherOwners && userId != 0)
				return Task.FromResult(PreconditionResult.FromSuccess());
			if (!_allowOtherOwners && userId != 0)
				return Task.FromResult(PreconditionResult.FromError(
					"Sorry, but only the primary owner of this Discord server can run this command!"));

			return Task.FromResult(
				PreconditionResult.FromError(
					"You are not a owner of this Discord server, you cannot run this command!"));
		}
	}
}