using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Preconditions
{
	public class CooldownAttribute : PreconditionAttribute
	{
		TimeSpan CooldownLength { get; set; }
		readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns = new ConcurrentDictionary<CooldownInfo, DateTime>();

		public CooldownAttribute(int seconds)
		{
			CooldownLength = TimeSpan.FromSeconds(seconds);
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var key = new CooldownInfo(context.User.Id, command.GetHashCode());
			
			if (_cooldowns.TryGetValue(key, out DateTime endsAt))
			{
				var difference = endsAt.Subtract(DateTime.Now);
				if (difference.Ticks > 0)
				{
					return Task.FromResult(PreconditionResult.FromError($"Please wait {difference.ToString(@"ss")} seconds before trying again!"));
				}

				var time = DateTime.Now.Add(CooldownLength);
				_cooldowns.TryUpdate(key, time, endsAt);
			}
			else
			{
				_cooldowns.TryAdd(key, DateTime.Now.Add(CooldownLength));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
