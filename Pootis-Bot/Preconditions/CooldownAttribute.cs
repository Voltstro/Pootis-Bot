using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Preconditions
{
	public class CooldownAttribute : PreconditionAttribute
	{
		private readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns =
			new ConcurrentDictionary<CooldownInfo, DateTime>();

		/// <summary>
		/// Creates a new <see cref="CooldownAttribute"/>
		/// </summary>
		/// <param name="seconds">Yo waddup my dude?</param>
		public CooldownAttribute(int seconds)
		{
			CooldownLength = TimeSpan.FromSeconds(seconds);
		}

		private TimeSpan CooldownLength { get; }

		/// <summary>
		/// Checks if a user is on cooldown
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
			IServiceProvider services)
		{
			CooldownInfo key = new CooldownInfo(context.User.Id, command.GetHashCode());

			if (_cooldowns.TryGetValue(key, out DateTime endsAt))
			{
				TimeSpan difference = endsAt.Subtract(DateTime.Now);
				if (difference.Ticks > 0)
					return Task.FromResult(
						PreconditionResult.FromError(
							$"Please wait {difference:ss} seconds before trying again!"));

				DateTime time = DateTime.Now.Add(CooldownLength);
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