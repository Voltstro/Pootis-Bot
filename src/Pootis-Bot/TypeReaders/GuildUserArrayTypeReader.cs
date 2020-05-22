using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Pootis_Bot.TypeReaders
{
	/// <summary>
	/// A <see cref="TypeReader"/> for parsing objects implementing <see cref="SocketGuildUser"/> arrays.
	/// </summary>
	public class GuildUserArrayTypeReader : TypeReader
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
			IServiceProvider services)
		{
			string[] users = input.Split(new[] {", ", ",", " ,", " , "}, StringSplitOptions.RemoveEmptyEntries);
			List<SocketGuildUser> results = new List<SocketGuildUser>();
			IReadOnlyCollection<IGuildUser> guildUsers =
				context.Guild.GetUsersAsync(CacheMode.CacheOnly).GetAwaiter().GetResult();

			foreach (string userList in users)
			{
				string user = userList;
				if (user.StartsWith(' '))
					user = user.TrimStart(' ');

				if (user.EndsWith(' '))
					user = user.TrimEnd(' ');

				//By mention
				if (MentionUtils.TryParseUser(user, out ulong id))
				{
					SocketGuildUser guildUser = GetUser(id, context);
					if (guildUser == null)
						return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound,
							"User not found."));

					results.Add(guildUser);
					continue;
				}

				//By Id
				if (ulong.TryParse(user, NumberStyles.None, CultureInfo.InvariantCulture, out id))
				{
					SocketGuildUser guildUser = GetUser(id, context);
					if (guildUser == null)
						return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound,
							"User not found."));

					results.Add(guildUser);
					continue;
				}

				//By Username + Discriminator
				int index = user.LastIndexOf('#');
				if (index >= 0)
				{
					string username = user.Substring(0, index);
					if (!ushort.TryParse(user.Substring(index + 1), out ushort discriminator)) continue;

					SocketGuildUser guildUser = (SocketGuildUser) guildUsers.FirstOrDefault(x =>
						x.DiscriminatorValue == discriminator &&
						string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));

					if (guildUser == null)
						return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound,
							"User not found."));

					results.Add(guildUser);
					continue;
				}

				bool userFound = false;

				//By Username
				foreach (IGuildUser guildUser in guildUsers.Where(x =>
					string.Equals(user, x.Username, StringComparison.OrdinalIgnoreCase)))
				{
					results.Add((SocketGuildUser) guildUser);
					userFound = true;
				}

				//By Nickname
				foreach (IGuildUser guildUser in guildUsers.Where(x =>
					string.Equals(user, x.Nickname, StringComparison.OrdinalIgnoreCase)))
				{
					results.Add((SocketGuildUser) guildUser);
					userFound = true;
				}

				if (!userFound)
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound,
						"User not found."));
			}

			return Task.FromResult(results.Count > 0
				? TypeReaderResult.FromSuccess(results.ToArray())
				: TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found."));
		}

		private static SocketGuildUser GetUser(ulong id, ICommandContext context)
		{
			return (SocketGuildUser) context.Guild.GetUserAsync(id, CacheMode.CacheOnly).GetAwaiter().GetResult();
		}
	}
}