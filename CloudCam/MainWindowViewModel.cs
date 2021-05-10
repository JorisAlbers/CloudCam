using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class MainWindowViewModel : ReactiveObject
    {
        private SettingsSerializer _settingsSerializer;

        [Reactive] 
        public SettingsViewModel SettingsViewModel { get; set; }

        public MainWindowViewModel()
        {
            string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudCam");

            _settingsSerializer = new SettingsSerializer(new FileInfo(Path.Combine(rootFolder, "settings.json")));
            SettingsViewModel = new SettingsViewModel(_settingsSerializer);


        }
    }
}
