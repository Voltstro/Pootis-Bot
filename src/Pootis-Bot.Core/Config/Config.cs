using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Config
{
	/// <summary>
	///     Configs allow to save and load settings
	/// </summary>
	/// <typeparam name="T">The class of settings to save</typeparam>
	public class Config<T> where T : Config<T>, new()
	{
		private static readonly string ConfigPath = $"Config/{typeof(T).Name}.json";
		private static T instance;

		/// <summary>
		///     The instance of this <see cref="Config{T}" />.
		///     <para>If the config file doesn't exist, it will create a new <see cref="T" /> and save the file.</para>
		///     <para>If the config file does exist, then it will load it.</para>
		/// </summary>
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					if (File.Exists(ConfigPath))
					{
						Logger.Debug("Loaded config {@Config} from {@ConfigLocation}", typeof(T).Name, ConfigPath);
						instance = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));
					}
					else
					{
						Logger.Debug("Created new {@Config} instance.", typeof(T).Name);
						instance = new T();
						instance.Save();
					}
				}

				return instance;
			}
			set
			{
				instance = value;
				instance.Save();
			}
		}

		/// <summary>
		///     Saves the config to the disk
		/// </summary>
		public void Save()
		{
			if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? string.Empty);

			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(ConfigPath, json);
		}
	}
}