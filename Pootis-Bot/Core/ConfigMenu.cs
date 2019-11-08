using System;

namespace Pootis_Bot.Core
{
	public class ConfigMenu
	{
		public class ConfigResult
		{
			public enum ResultTypes {Token, Prefix, Name}

			public bool WasModified { get; set; }
			public ResultTypes ResultType { get; set; }
		}

		public void OpenConfig(bool isFirstStartUp = false)
		{
			Global.Log("Entering config menu...");

			Console.WriteLine("");
			Console.WriteLine("---------------------------------------------------------");
			Console.WriteLine("                    Bot configuration                    ");
			Console.WriteLine("---------------------------------------------------------");
			Console.WriteLine("1 - Bot Token");
			Console.WriteLine("2 - Bot Prefix");
			Console.WriteLine("3 - Bot Name");
			//Console.WriteLine("4 - APIs");
			Console.WriteLine("");
			
			Console.WriteLine("Enter in either '1', '2' ect... or 'exit' to exit the config menu.");

			ConfigResult tokenResult = new ConfigResult{ResultType = ConfigResult.ResultTypes.Token, WasModified = false};

			ConfigResult prefixResult = new ConfigResult{ResultType = ConfigResult.ResultTypes.Prefix, WasModified = false};

			ConfigResult nameResult = new ConfigResult{ResultType = ConfigResult.ResultTypes.Name, WasModified = false};

			bool somethingWasModified = false;

			while (true)
			{
				string input = Console.ReadLine()?.ToLower().Trim();

				switch (input)
				{
					case "1":
					{
						tokenResult = ConfigEditToken();
						if (tokenResult.WasModified) somethingWasModified = true;
						break;
					}
					case "2":
					{
						prefixResult = ConfigEditPrefix();
						if(prefixResult.WasModified) somethingWasModified = true;
						break;
					}
					case "3":
					{
						nameResult = ConfigEditName();
						if(nameResult.WasModified) somethingWasModified = true;
						break;
					}
					case "exit":
					{
						//If it is the first startup, check to make sure the token was modified.
						if (isFirstStartUp && !tokenResult.WasModified)
						{
							//The token was not modified
							Console.WriteLine("You need to modify the token since this is the first startup!");
							break;
						}

						//The token was modified, and it isn't the first startup, so alert the user they need to restart the bot for the new token to take effect
						if (!isFirstStartUp && tokenResult.WasModified)
						{
							Global.Log("For the bot to use the new token you must restart the bot!", ConsoleColor.Yellow);
						}

						//Set the new prefix
						if (prefixResult.WasModified)
						{
							Global.BotPrefix = Config.bot.BotPrefix;
						}

						if (nameResult.WasModified)
						{
							Global.BotName = Config.bot.BotName;
							Console.Title = $"{Global.BotName} Console";
						}

						//No point in saving if nothing was changed
						if (somethingWasModified)
						{
							Config.SaveConfig();
							Global.Log("Config has been saved!");
						}
						else
						{
							Global.Log("Nothing was changed, so nothing was saved!");
						}

						return;
					}
					default:
					{
						Console.WriteLine("Input either needs to be '1', '2', ect... or 'exit' to exit the config menu.");
						break;
					}
				}
			}
		}

		private static ConfigResult ConfigEditToken()
		{
			Console.WriteLine("Enter in a new token for the bot to use.");
			Console.WriteLine($"The current token is `{Global.BotToken}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");

			while (true)
			{
				string newToken = Console.ReadLine()?.Trim();

				if (newToken == "exit")
				{
					Console.WriteLine("The token was not modified.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Token, WasModified = false};
				}
				if(string.IsNullOrWhiteSpace(newToken))
				{
					Console.WriteLine("The token cannot be blank!");
				}
				else
				{
					Config.bot.BotToken = newToken;
					Global.BotToken = newToken;
					Console.WriteLine($"The token will be set to `{newToken}`.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Token, WasModified = true};
				}
			}
		}

		private static ConfigResult ConfigEditPrefix()
		{
			Console.WriteLine("Enter in a new prefix for the bot to use.");
			Console.WriteLine($"The current prefix is `{Global.BotPrefix}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");

			while (true)
			{
				string newPrefix = Console.ReadLine()?.Trim();

				if (newPrefix == "exit")
				{
					Console.WriteLine("The prefix was not modified.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Prefix, WasModified = false};
				}
				if(string.IsNullOrWhiteSpace(newPrefix))
				{
					Console.WriteLine("The prefix cannot be blank!");
				}
				else
				{
					Config.bot.BotPrefix = newPrefix;
					Console.WriteLine($"The prefix will be set to `{newPrefix}`.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Prefix, WasModified = true};
				}
			}
		}

		private static ConfigResult ConfigEditName()
		{
			Console.WriteLine("Enter in a new name for the bot to use.");
			Console.WriteLine($"The current name of the bot is `{Global.BotName}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");

			while (true)
			{
				string newName = Console.ReadLine()?.Trim();

				if (newName == "exit")
				{
					Console.WriteLine("The bot name was not modified.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Name, WasModified = false};
				}
				if(string.IsNullOrWhiteSpace(newName))
				{
					Console.WriteLine("The bot name cannot be blank!");
				}
				else
				{
					Config.bot.BotName = newName;
					Console.WriteLine($"The bot name will be set to `{newName}`.");
					return new ConfigResult{ResultType = ConfigResult.ResultTypes.Name, WasModified = true};
				}
			}
		}
	}
}
