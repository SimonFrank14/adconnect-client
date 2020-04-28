using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;

namespace ConnectClient.Core.Settings
{
    public static class SettingsManager
    {
        public static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SchulIT", "AD Connect Client", "settings.json");
        }

        public static JsonSettings LoadSettings()
        {
            var file = GetPath();

            if (!File.Exists(file))
            {
                using var stream = new StreamWriter(file);
                stream.Write(JsonConvert.SerializeObject(new JsonSettings(), Formatting.Indented));
            }

            using var reader = new StreamReader(file);
            var settings = reader.ReadToEnd();
            var settingsObj = JsonConvert.DeserializeObject<JsonSettings>(settings);

            return settingsObj;
        }
    }
}
