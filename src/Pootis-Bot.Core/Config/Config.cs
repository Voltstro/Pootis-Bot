using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Pootis_Bot.Console.ConfigMenus;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Config
{
	//TODO: Add property 'Validators', to make a property isn't what it should be allowed to be. E.G: The token cannot be null or empty

	/// <summary>
	///     Configs allow to save and load settings
	/// </summary>
	/// <typeparam name="T">The class of settings to save</typeparam>
	public class Config<T> where T : Config<T>, new()
	{
		/// <summary>
		///     What is the expected config version
		/// </summary>
		// ReSharper disable once StaticMemberInGenericType
		[PublicAPI] public static int ExpectedConfigVersion = 1;

		private static readonly string ConfigPath = $"Config/{typeof(T).Name}.json";
		private static T instance;

		/// <summary>
		///     Whats the version that this config is.
		///     <para>If you want to update the config version, change <see cref="Config{T}.ExpectedConfigVersion" /></para>
		/// </summary>
		[PublicAPI]
		[DontShowItem]
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
		///     Saves the config to the disk
		/// </summary>
		public void Save()
		{
			if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? string.Empty);

			string json = ToJson();
			File.WriteAllText(ConfigPath, json);
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
			if (File.Exists(ConfigPath))
			{
				Logger.Debug("Loaded config {@Config} from {@ConfigLocation}", typeof(T).Name, ConfigPath);
				instance = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));

				if (instance.ConfigVersion != ExpectedConfigVersion)
				{
					Logger.Warn("Config {@CConfig} was an outdated version! Updating.", typeof(T).Name);
					instance.ConfigVersion = ExpectedConfigVersion;
					instance.Save();
				}
			}
			else
			{
				Logger.Debug("Created new config {@Config} instance.", typeof(T).Name);
				instance = new T();
				instance.Save();
			}
		}
	}
}