using System;
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

            try
            {
                using StreamReader reader = _settingsFile.OpenText();
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(data);
            }
            catch (Exception e)
            {
                
                var rootFolder = _settingsFile.Directory;
                return new Settings(new DirectoryInfo(Path.Combine(rootFolder.FullName, "Frames")),
                    new DirectoryInfo(Path.Combine(rootFolder.FullName, "Mustaches")),
                    new DirectoryInfo(Path.Combine(rootFolder.FullName, "Output")));
            }
        }

        public void Save(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings);
            using StreamWriter writer = new StreamWriter(_settingsFile.OpenWrite());
            writer.Write(json);
        }
    }
}