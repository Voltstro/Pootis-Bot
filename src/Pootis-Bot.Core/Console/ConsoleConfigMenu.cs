using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Pootis_Bot.Console
{
	public class ConsoleConfigMenu<T> : IDisposable
	{
		private List<ConfigItem> configMenu;
		private bool showingMenu;

		private T options;

		public ConsoleConfigMenu(T options)
		{
			configMenu = new List<ConfigItem>();
			this.options = options;

			//Generate options
			foreach (FieldInfo field in typeof(T).GetFields())
			{
				string formatName = field.Name;

				if (Attribute.GetCustomAttribute(field, typeof(ConsoleConfigFormat)) is ConsoleConfigFormat attribute)
					formatName = attribute.FormattedName;

				configMenu.Add(new ConfigItem
				{
					configFormatName = formatName,
					field = field
				});
			}
		}

		public void Show()
		{
			showingMenu = true;
			StringBuilder options = new StringBuilder();
			for (int i = 0; i < configMenu.Count; i++)
			{
				options.Append($"{i} - {configMenu[i].configFormatName}");
			}

			System.Console.WriteLine(options.ToString());
			while (showingMenu)
			{
				string input = System.Console.ReadLine();
				if (input.ToLower() == "exit")
				{
					System.Console.WriteLine("Exiting config menu...");
					showingMenu = false;
					break;
				}

				if (int.TryParse(input, out int menu))
				{
					if (menu > configMenu.Count)
					{
						System.Console.WriteLine($"Input number cannot be greater then {configMenu.Count}!");
						continue;
					}

					EditField(configMenu[menu]);
				}
				else
				{
					System.Console.WriteLine("Input either needs to be '1', '2', '3', etc... or 'exit'.");
				}
			}
		}

		public void Close()
		{
			showingMenu = false;
		}

		public void Dispose()
		{
		}

		private void EditField(ConfigItem item)
		{
			while (true)
			{
				System.Console.WriteLine($"Enter what you want to set {item.configFormatName} to:");
				string input = System.Console.ReadLine();

				if (item.field.FieldType == typeof(string))
				{
					item.field.SetValue(options, input);
					break;
				}
				else if(item.field.FieldType == typeof(bool))
				{
					if (bool.TryParse(input.ToLower(), out bool value))
					{
						item.field.SetValue(options, value);
						break;
					}

					System.Console.WriteLine("Input either needs to be 'true' or 'false'!");
				}
			}

		}

		private struct ConfigItem
		{
			public string configFormatName;
			public FieldInfo field;
		}
	}
}