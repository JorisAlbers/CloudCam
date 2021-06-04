using System.IO;
using Newtonsoft.Json;

namespace CloudCam
{
    public class SettingsSerializer
    {
        private readonly FileInfo _settingsFile;
        
        public SettingsSerializer(FileInfo settingsFile)
        {
            _settingsFile = settingsFile;
        }

        public Settings Load()
        {
                using StreamReader reader = _settingsFile.OpenText();
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(data);
        }

        public void Save(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings);
            using StreamWriter writer = new StreamWriter(_settingsFile.Create());
            writer.Write(json);
        }
    }
}