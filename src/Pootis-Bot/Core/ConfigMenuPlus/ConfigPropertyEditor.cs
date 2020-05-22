using System;
using System.Reflection;
using Pootis_Bot.Attributes;
using Pootis_Bot.Core.Logging;

namespace Pootis_Bot.Core.ConfigMenuPlus
{
	public static class ConfigPropertyEditor
	{
		public static string EditField<T>(string configPropertyName, object propertyObject)
		{
			Type type = typeof(T);
			PropertyInfo prop = type.GetProperty(configPropertyName);
			if (prop == null)
			{
				Logger.Log($"The config property '{configPropertyName}' doesn't exist!", LogVerbosity.Error);
				return null;
			}

			string configName = configPropertyName;
			if (prop.GetCustomAttribute<ConfigMenuNameAttribute>() != null)
				configName = prop.GetCustomAttribute<ConfigMenuNameAttribute>().FormattedName;

			Console.WriteLine(
				$"Input what you want to change {configName} to. Its current value is `{prop.GetValue(propertyObject)}`");
			string input = Console.ReadLine();

			prop.SetValue(propertyObject, input, null);

			Console.WriteLine($"{configName} was set to `{input}`.");
			return input;
		}
	}
}