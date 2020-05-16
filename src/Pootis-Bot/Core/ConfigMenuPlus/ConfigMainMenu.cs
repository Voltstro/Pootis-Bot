using System;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Entities;
using Console = Pootis_Bot.ConsoleCommandHandler.Console;

namespace Pootis_Bot.Core.ConfigMenuPlus
{
	public class ConfigMainMenu : Console
	{
		public void OpenConfigMenu()
		{
			UnknownCommandError = "Unknown input! It either needs to be '1', '2', etc or 'exit' to exit out of the config.";
			UnknownCommandErrorColor = ConsoleColor.Red;

			//Intro stuff
			System.Console.WriteLine("");
			System.Console.WriteLine("---------------------------------------------------------");
			System.Console.WriteLine("                    Bot configuration                    ");
			System.Console.WriteLine("---------------------------------------------------------");
			System.Console.WriteLine("1 - Bot Token");
			System.Console.WriteLine("2 - Bot Prefix");
			System.Console.WriteLine("3 - Bot Name");
			System.Console.WriteLine("4 - APIs");
			System.Console.WriteLine("");
			System.Console.WriteLine("Enter in either '1', '2' ect... or 'exit' to exit the config menu.");

			// -- Bot token command
			AddCommand("1", "", EditToken);

			// -- Bot prefix command
			AddCommand("2", "", EditPrefix);

			// -- Bot name command
			AddCommand("3", "", EditName);

			// -- API menu
			AddCommand("4", "", () => new ConfigApiMenu().OpenApiMenu());

			// -- Exit command
			AddCommand("exit", "", () =>
			{
				IsExiting = true;
				Logger.Log("Exited out of config menu.");
			});

			ConsoleHandleLoop();
		}

		private void EditToken()
		{
			while (true)
			{
				string token = ConfigPropertyEditor.EditField<ConfigFile>(nameof(Config.bot.BotToken), Config.bot);
				
				if (string.IsNullOrWhiteSpace(token))
				{
					System.Console.WriteLine("A token must be imputed!");
					continue;
				}

				Config.SaveConfig();

				if (string.IsNullOrWhiteSpace(Global.BotToken))
					Global.BotToken = token;
				else
					System.Console.WriteLine("You will need to restart the bot to use the new token!");
				break;
			}
		}

		private void EditPrefix()
		{
			while (true)
			{
				string prefix = ConfigPropertyEditor.EditField<ConfigFile>(nameof(Config.bot.BotPrefix), Config.bot);

				if (string.IsNullOrWhiteSpace(prefix))
				{
					System.Console.WriteLine("A prefix must be imputed!");
					continue;
				}

				Config.SaveConfig();
				Global.BotPrefix = prefix;
				break;
			}
		}

		private void EditName()
		{
			while (true)
			{
				string name = ConfigPropertyEditor.EditField<ConfigFile>(nameof(Config.bot.BotName), Config.bot);

				if (string.IsNullOrWhiteSpace(name))
				{
					System.Console.WriteLine("A name must be imputed!");
					continue;
				}

				Config.SaveConfig();
				Global.BotName = name;
				break;
			}
		}

		public override void LogMessage(string message, ConsoleColor color)
		{
			ConsoleColor currentColor = System.Console.ForegroundColor;
			System.Console.ForegroundColor = color;
			System.Console.WriteLine(message);
			System.Console.ForegroundColor = currentColor;
		}
	}
}