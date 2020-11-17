using System.IO;
using Newtonsoft.Json;

namespace Pootis_Bot.Config
{
	public class Config<T> where T : Config<T>, new()
	{
		private static readonly string ConfigPath = $"Config/{typeof(T).Name}.json";
		private static T instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					if (File.Exists(ConfigPath))
						instance = JsonConvert.DeserializeObject<T>(File.ReadAllText(ConfigPath));
					else
					{
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

		public void Save()
		{
			if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(ConfigPath, json);
		}
	}
}