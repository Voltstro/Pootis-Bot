using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Pootis_Bot.Preconditions
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class RequireGuildOwnerAttribute : PreconditionAttribute
	{
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
			if (context.User.Id == context.Guild.OwnerId)
				return Task.FromResult(PreconditionResult.FromSuccess());
			return Task.FromResult(
				PreconditionResult.FromError(
					"You are not the owner of this Discord server, you cannot run this command!"));
		}
	}
}