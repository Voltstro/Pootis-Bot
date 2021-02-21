using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Console.ConfigMenus;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Config
{
	//TODO: Add property 'Validators', to make sure a property isn't what it shouldn't be allowed to be. E.G: The token cannot be null or empty

	/// <summary>
	///     Configs allow to save and load settings
	/// </summary>
	/// <typeparam name="T">The class of settings to save</typeparam>
	public class Config<T> where T : Config<T>, new()
	{
		/// <summary>
		///     What is the expected config version
		///		<para>This can be used if you add some extra options to the config in a later release.</para>
		///		<para>Change this before accessing <see cref="Instance"/> (or do <see cref="Reload"/>), which will cause a config upgrade</para>
		/// </summary>
		// ReSharper disable once StaticMemberInGenericType
		public static int ExpectedConfigVersion = 1;

		private static readonly string ConfigPath = $"Config/{typeof(T).Name}.json";
		private static T instance;

		/// <summary>
		///     Whats the version that this config is.
		///     <para>If you want to update the config version, change <see cref="Config{T}.ExpectedConfigVersion" /></para>
		/// </summary>
		[DontShowItem]
		[JsonProperty]
		public int ConfigVersion { get; private set; } = ExpectedConfigVersion;

		/// <summary>
		///     The instance of this <see cref="Config{T}" />.
		///     <para>If the config file doesn't exist, it will create a new <see cref="T" /> and save the file.</para>
		///     <para>If the config file does exist, then it will load it.</para>
		/// </summary>
		public static T Instance
		{
			get
			{
				if (instance == null) StaticReload();

				return instance;
			}
		}

		/// <summary>
		///		Invoked when this config is saved
		/// </summary>
		public event Action Saved;

		/// <summary>
		///     Saves the config to the disk
		/// </summary>
		public void Save()
		{
			if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? string.Empty);

			string json = ToJson();
			File.WriteAllText(ConfigPath, json);
			Saved?.Invoke();
		}

		/// <summary>
		///     Converts the config to json
		/// </summary>
		/// <returns></returns>
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		/// <summary>
		///     Reloads the config
		/// </summary>
		public void Reload()
		{
			StaticReload();
		}

		private static void StaticReload()
		{
			if (File.Exists(ConfigPath)) //If the config file already exists
			{
				Logger.Debug("Loaded config {@Config} from {@ConfigLocation}", typeof(T).Name, ConfigPath);
				instance = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));

				//If the current config version doesn't meet what is expected then we need to re-save it with the new options
				if (instance.ConfigVersion == ExpectedConfigVersion) return;

				Logger.Warn("Config {@CConfig} was an outdated version! Updating.", typeof(T).Name);
				instance.ConfigVersion = ExpectedConfigVersion;
				instance.Save();
			}
			else //If it doesn't then we need to create a new one and write it to disk
			{
				Logger.Debug("Created new config {@Config} instance.", typeof(T).Name);
				instance = new T {ConfigVersion = ExpectedConfigVersion};
				instance.Save();
			}
		}
	}
}