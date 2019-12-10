using System;

namespace Pootis_Bot.Core
{
	public class ConfigMenu
	{
		/// <summary>
		/// Open up the config menu
		/// </summary>
		/// <param name="isFirstStartUp">Forces the user to set a token</param>
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
			Console.WriteLine("4 - APIs");
			Console.WriteLine("");

			Console.WriteLine("Enter in either '1', '2' ect... or 'exit' to exit the config menu.");

			ConfigResult tokenResult = new ConfigResult
				{ResultType = ConfigResult.ResultTypes.Token, WasModified = false};
			ConfigResult prefixResult = new ConfigResult
				{ResultType = ConfigResult.ResultTypes.Prefix, WasModified = false};
			ConfigResult nameResult = new ConfigResult
				{ResultType = ConfigResult.ResultTypes.Name, WasModified = false};

			bool somethingWasModified = false;

			while (true)
			{
				string input = Console.ReadLine()?.ToLower().Trim();

				switch (input)
				{
					case "1": //Bot token
					{
						tokenResult = ConfigEditToken();
						if (tokenResult.WasModified) somethingWasModified = true;
						break;
					}
					case "2": //Bot prefix
					{
						prefixResult = ConfigEditPrefix();
						if (prefixResult.WasModified) somethingWasModified = true;
						break;
					}
					case "3": //Bot name
					{
						nameResult = ConfigEditName();
						if (nameResult.WasModified) somethingWasModified = true;
						break;
					}
					case "4": //API keys
					{
						ConfigResult apiResult = ConfigEditApis();
						if (apiResult.WasModified) somethingWasModified = true;
						break;
					}
					case "exit": //Exit out of config menu
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
							Global.Log("For the bot to use the new token you must restart the bot!",
								ConsoleColor.Yellow);

						//Set the new prefix
						if (prefixResult.WasModified) Global.BotPrefix = Config.bot.BotPrefix;

						//Update the console title if the bot name was changed
						if (nameResult.WasModified)
						{
							Global.BotName = Config.bot.BotName;
							Console.Title = $"{Global.BotName} Console";
						}

						//No point in saving if nothing was changed
						if (somethingWasModified)
						{
							Config.SaveConfig();
							Global.Log("Config has been saved! Exited out of the config menu.");
						}
						else
						{
							Global.Log("Nothing was changed, so nothing was saved! Exited out of the config menu.");
						}

						return;
					}
					default:
					{
						Console.WriteLine(
							"Input either needs to be '1', '2', ect... or 'exit' to exit the config menu.");
						break;
					}
				}
			}
		}

		private static ConfigResult ConfigEditApis()
		{
			Console.WriteLine("APIs are needed for commands such as `google`, `youtube`, `giphy`");
			Console.WriteLine("It is definitely recommended.");
			Console.WriteLine("");
			Console.WriteLine("1 - Giphy API Key");
			Console.WriteLine("2 - Youtube API Key");
			Console.WriteLine("3 - Google API Key");
			Console.WriteLine("4 - Google Search Id");
			Console.WriteLine("");
			Console.WriteLine("At any time type 'return' to return back to the bot configuration menu.");

			string giphyKey = Config.bot.Apis.ApiGiphyKey;
			string youtubeKey = Config.bot.Apis.ApiYoutubeKey;
			string googleKey = Config.bot.Apis.ApiGoogleSearchKey;
			string googleSearchId = Config.bot.Apis.GoogleSearchEngineId;

			while (true)
			{
				string input = Console.ReadLine()?.Trim();

				switch (input)
				{
					case "1":
						giphyKey = ConfigEditApiGiphy();
						break;
					case "2":
						youtubeKey = ConfigEditApiYouTube();
						break;
					case "3":
						googleKey = ConfigEditApiGoogleSearch();
						break;
					case "4":
						googleSearchId = ConfigEditApiGoogleEngineId();
						break;
					case "return":
					{
						bool isModified = giphyKey != Config.bot.Apis.ApiGiphyKey ||
						                  youtubeKey != Config.bot.Apis.ApiYoutubeKey ||
						                  googleKey != Config.bot.Apis.ApiGoogleSearchKey ||
						                  googleSearchId != Config.bot.Apis.GoogleSearchEngineId;

						if (isModified)
						{
							Config.bot.Apis.ApiGiphyKey = giphyKey;
							Config.bot.Apis.ApiYoutubeKey = youtubeKey;
							Config.bot.Apis.ApiGoogleSearchKey = googleKey;
							Config.bot.Apis.GoogleSearchEngineId = googleSearchId;

							Console.WriteLine(
								"API keys have immediately been updated, but are not saved until the config menu is exited. Exited back to the main config menu.");
							return new ConfigResult {ResultType = ConfigResult.ResultTypes.Apis, WasModified = true};
						}

						Console.WriteLine("Nothing was changed, exited back to the main config menu.");
						return new ConfigResult {ResultType = ConfigResult.ResultTypes.Apis, WasModified = false};
					}
					default:
						Console.WriteLine(
							"Input either needs to be '1', '2', ect... or 'return' to exit to the config menu.");
						break;
				}
			}
		}

		public class ConfigResult
		{
			public enum ResultTypes
			{
				Token,
				Prefix,
				Name,
				Apis
			}

			/// <summary>
			/// Was the existing result modified?
			/// </summary>
			public bool WasModified { get; set; }

			/// <summary>
			/// What was the result type (Token, Prefix, etc...)
			/// </summary>
			public ResultTypes ResultType { get; set; }
		}

		#region Config Edit Token, Name, Prefix

		private static ConfigResult ConfigEditToken()
		{
			Console.WriteLine($"The current token is `{Global.BotToken}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");
			Console.WriteLine("Enter in a new token for the bot to use:");

			while (true)
			{
				string newToken = Console.ReadLine()?.Trim();

				if (newToken == "exit")
				{
					Console.WriteLine("The token was not modified.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Token, WasModified = false};
				}

				if (string.IsNullOrWhiteSpace(newToken))
				{
					Console.WriteLine("The token cannot be blank!");
				}
				else
				{
					Config.bot.BotToken = newToken;
					Global.BotToken = newToken;
					Console.WriteLine($"The token will be set to `{newToken}`.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Token, WasModified = true};
				}
			}
		}

		private static ConfigResult ConfigEditPrefix()
		{
			Console.WriteLine($"The current prefix is `{Global.BotPrefix}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");
			Console.WriteLine("Enter in a new prefix for the bot to use:");

			while (true)
			{
				string newPrefix = Console.ReadLine()?.Trim();

				if (newPrefix == "exit")
				{
					Console.WriteLine("The prefix was not modified.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Prefix, WasModified = false};
				}

				if (string.IsNullOrWhiteSpace(newPrefix))
				{
					Console.WriteLine("The prefix cannot be blank!");
				}
				else
				{
					Config.bot.BotPrefix = newPrefix;
					Console.WriteLine($"The prefix will be set to `{newPrefix}`.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Prefix, WasModified = true};
				}
			}
		}

		private static ConfigResult ConfigEditName()
		{
			Console.WriteLine($"The current name of the bot is `{Global.BotName}`.");
			Console.WriteLine("To exit without saving, type in `exit`.");
			Console.WriteLine("Enter in a new name for the bot to use:");

			while (true)
			{
				string newName = Console.ReadLine()?.Trim();

				if (newName == "exit")
				{
					Console.WriteLine("The bot name was not modified.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Name, WasModified = false};
				}

				if (string.IsNullOrWhiteSpace(newName))
				{
					Console.WriteLine("The bot name cannot be blank!");
				}
				else
				{
					Config.bot.BotName = newName;
					Console.WriteLine($"The bot name will be set to `{newName}`.");

					return new ConfigResult {ResultType = ConfigResult.ResultTypes.Name, WasModified = true};
				}
			}
		}

		#endregion

		#region Config APIs

		private static string ConfigEditApiGiphy()
		{
			Console.WriteLine($"The current Giphy API key is `{Config.bot.Apis.ApiGiphyKey}`.");
			Console.WriteLine("Enter in a Giphy API key:");

			string giphyKey = Console.ReadLine()?.Trim();

			Console.WriteLine($"Giphy API key has been set to `{giphyKey}`.");

			return giphyKey;
		}

		private static string ConfigEditApiYouTube()
		{
			Console.WriteLine($"The current YouTube API key is `{Config.bot.Apis.ApiYoutubeKey}`.");
			Console.WriteLine("Enter in a YouTube API key:");

			string youTubeKey = Console.ReadLine()?.Trim();

			Console.WriteLine($"YouTube API key has been set to `{youTubeKey}`.");

			return youTubeKey;
		}

		private static string ConfigEditApiGoogleSearch()
		{
			Console.WriteLine($"The current Google search API key is `{Config.bot.Apis.ApiGoogleSearchKey}`.");
			Console.WriteLine("Enter in a Google search API key:");

			string googleKey = Console.ReadLine()?.Trim();

			Console.WriteLine($"Google search API key has been set to `{googleKey}`.");

			return googleKey;
		}

		private static string ConfigEditApiGoogleEngineId()
		{
			Console.WriteLine($"The current Google Engine ID is `{Config.bot.Apis.GoogleSearchEngineId}`.");
			Console.WriteLine("Enter in a Google Engine ID:");

			string googleId = Console.ReadLine()?.Trim();

			Console.WriteLine($"Google Engine ID has been set to `{googleId}`.");

			return googleId;
		}

		#endregion
	}
}