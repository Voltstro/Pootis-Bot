using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Pootis_Bot.Preconditions
{
	//
	// Summary:
	//     Requires the command to be invoked by the owner of the Discord server.
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class RequireGuildOwnerAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context.User.Id == context.Guild.OwnerId)
				return Task.FromResult(PreconditionResult.FromSuccess());
			else
				return Task.FromResult(PreconditionResult.FromError("You are not the owner of this Discord server!"));
		}
	}
}
