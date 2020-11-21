using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Pootis_Bot.Console.ConfigMenus
{
	public class ConsoleConfigMenu<T>
	{
		private readonly List<ConfigItem> configMenu;
		private readonly T editingObject;

		private bool showingMenu;

		public ConsoleConfigMenu(T editingObject)
		{
			configMenu = new List<ConfigItem>();
			this.editingObject = editingObject;

			//Generate options
			foreach (PropertyInfo property in typeof(T).GetProperties())
			{
				if(Attribute.GetCustomAttribute(property, typeof(DontShowItem)) != null)
					continue;

				string formatName = property.Name;

				if (Attribute.GetCustomAttribute(property, typeof(MenuItemFormat)) is MenuItemFormat attribute)
					formatName = attribute.FormattedName;

				configMenu.Add(new ConfigItem
				{
					ConfigFormatName = formatName,
					Property = property
				});
			}
		}

		public void Show()
		{
			showingMenu = true;
			StringBuilder options = new StringBuilder();
			for (int i = 0; i < configMenu.Count; i++)
			{
				options.Append($"{i} - {configMenu[i].ConfigFormatName}\n");
			}

			System.Console.WriteLine(options.ToString());
			while (showingMenu)
			{
				string input = System.Console.ReadLine();
				if(input == null)
					continue;

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

		private void EditField(ConfigItem item)
		{
			string input;
			while (true)
			{
				System.Console.WriteLine($"Enter what you want to set {item.ConfigFormatName} to:");
				input = System.Console.ReadLine();

				if(input == null)
					continue;

				if (item.Property.PropertyType == typeof(string))
				{
					item.Property.SetValue(editingObject, input);
					break;
				}
				else if(item.Property.PropertyType == typeof(bool))
				{
					if (bool.TryParse(input.ToLower(), out bool value))
					{
						item.Property.SetValue(editingObject, value);
						break;
					}

					System.Console.WriteLine("Input either needs to be 'true' or 'false'!");
				}
			}

			System.Console.WriteLine($"{item.ConfigFormatName} was set to '{input}'.");
		}

		private struct ConfigItem
		{
			public string ConfigFormatName;
			public PropertyInfo Property;
		}
	}
}