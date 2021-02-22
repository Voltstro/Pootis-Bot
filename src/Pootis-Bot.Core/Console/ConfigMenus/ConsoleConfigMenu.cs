using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Console.ConfigMenus
{
	/// <summary>
	///     Allows to dynamically generate config menus
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConsoleConfigMenu<T>
	{
		private readonly List<ConfigItem> configMenu;
		private readonly string configTitle;
		private readonly T editingObject;

		private bool showingMenu;

		/// <summary>
		///     Creates a new <see cref="ConsoleConfigMenu{T}" /> instance
		/// </summary>
		/// <param name="editingObject"></param>
		public ConsoleConfigMenu([DisallowNull] T editingObject)
		{
			if (editingObject == null)
				throw new ArgumentNullException(nameof(editingObject));

			configMenu = new List<ConfigItem>();
			this.editingObject = editingObject;

			Type editingObjectType = typeof(T);

			configTitle = editingObjectType.Name;
			editingObjectType.GetCustomAttribute(typeof(MenuItemFormat));
			if (Attribute.GetCustomAttribute(editingObjectType, typeof(MenuItemFormat)) is MenuItemFormat titleFormat)
				configTitle = titleFormat.FormattedName;

			//Generate options
			foreach (PropertyInfo property in editingObjectType.GetProperties())
				try
				{
					if (Attribute.GetCustomAttribute(property, typeof(DontShowItem)) != null)
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
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred while setting up selection option for {Property}!",
						property.Name);
				}
		}

		/// <summary>
		///     Shows the generated config menu
		/// </summary>
		public void Show()
		{
			showingMenu = true;
			StringBuilder options = new StringBuilder();
			options.Append($"----==== {configTitle} ====----\n");

			for (int i = 0; i < configMenu.Count; i++) options.Append($"{i} - {configMenu[i].ConfigFormatName}\n");

			System.Console.WriteLine(options.ToString());
			while (showingMenu)
			{
				string input = System.Console.ReadLine();
				if (input == null)
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

		/// <summary>
		///     Closes the config menu
		/// </summary>
		public void Close()
		{
			showingMenu = false;
		}

		private void EditField(ConfigItem item)
		{
			string input = null;
			while (true)
				try
				{
					System.Console.WriteLine($"Enter what you want to set {item.ConfigFormatName} to:");
					input = System.Console.ReadLine();

					if (input == null)
						continue;

					//TODO: Add support for other types, we can do this using the same method of Discord.Net's 'TypeReaders'.
					if (item.Property.PropertyType == typeof(string))
					{
						item.Property.SetValue(editingObject, input);
						break;
					}

					if (item.Property.PropertyType == typeof(bool))
					{
						if (bool.TryParse(input.ToLower(), out bool value))
						{
							item.Property.SetValue(editingObject, value);
							break;
						}

						System.Console.WriteLine("Input either needs to be 'true' or 'false'!");
					}
				}
				catch (Exception ex)
				{
					Logger.Error("An error occurred while trying to set the value of {@Property}! {@ExMessage}",
						item.Property.Name, ex.Message);
					break;
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