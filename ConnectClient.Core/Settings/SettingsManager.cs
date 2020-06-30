using Newtonsoft.Json;
using System;
using System.IO;

namespace ConnectClient.Core.Settings
{
    public class SettingsManager
    {
        private JsonSettings settings;

        public static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SchulIT", "AD Connect Client", "settings.json");
        }

        public JsonSettings GetSettings()
        {
            return settings;
        }

        public void SaveSettings()
        {
            var file = GetPath();

            using var writer = new StreamWriter(file);
            writer.Write(JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public void LoadSettings()
        {
            var file = GetPath();

            if (!File.Exists(file))
            {
                using var stream = new StreamWriter(file);
                stream.Write(JsonConvert.SerializeObject(new JsonSettings(), Formatting.Indented));
            }

            using var reader = new StreamReader(file);
            var settings = reader.ReadToEnd();
            this.settings = JsonConvert.DeserializeObject<JsonSettings>(settings);
        }
    }
}
