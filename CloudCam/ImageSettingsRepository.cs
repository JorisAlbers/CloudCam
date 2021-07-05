using System;
using System.IO;
using Newtonsoft.Json;

namespace CloudCam
{
    public class ImageSettingsRepository
    {
        private string _folderPath;

        public ImageSettingsRepository(string folderPath)
        {
            _folderPath = folderPath;
        }

        public bool TryLoad(string imageName, out ImageSettings settings)
        {
            string fullName = Path.Combine(_folderPath, imageName);
            if(!File.Exists(fullName))
            {
                settings = null;
                return false;
            }

            string settingsPath = Path.ChangeExtension(fullName, ".json");
            if (!File.Exists(settingsPath))
            {
                settings = null;
                return false;
            }

            using StreamReader reader = new StreamReader(settingsPath);
            string data = reader.ReadToEnd();
            settings = JsonConvert.DeserializeObject<ImageSettings>(data);
            return true;
        }
    }

    public class ImageSettings
    {
        public int X { get; }
        public int Y { get; }
        public float Scale { get; }

        public ImageSettings(int x, int y, float scale)
        {
            X = x;
            Y = y;
            Scale = scale;
        }
    }
}