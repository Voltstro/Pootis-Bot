using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Pootis_Bot.Core;
using Pootis_Bot.Preconditions;
using Pootis_Bot.Services.Fun;
using Pootis_Bot.Structs;

namespace Pootis_Bot.Modules.Fun
{
	public class RandomPerson : ModuleBase<SocketCommandContext>
	{
		[Command("randomperson")]
		[Alias("person", "randperson")]
		[Summary("Generate a random person")]
		[Cooldown(5)]
		public async Task GenerateRandomPerson()
		{
			RandomPersonResults person = RandomPersonGenerator.GenerateRandomPerson();

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithTitle("Random Person");
			embed.AddField("Name", $"{person.PersonTitle} {person.PersonFirstName} {person.PersonLastName}");
			embed.AddField("Gender", Global.Title(person.PersonGender));
			embed.AddField("Location", $":flag_{person.CountryCode.ToLower()}: {person.City}, {person.State}, {person.Country}");
			embed.WithThumbnailUrl(person.PersonPicture);
			embed.WithColor(FunCmdsConfig.randomPersonColor);

			await Context.Channel.SendMessageAsync("", false, embed.Build());
		}
	}
}
