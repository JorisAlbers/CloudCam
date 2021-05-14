using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class MainWindowViewModel : ReactiveObject
    {
        private SettingsSerializer _settingsSerializer;
        private Settings _settings;
        private PhotoBoothViewModel _photoBoothViewModel;
        private SettingsViewModel _settingsViewModel { get; set; }
        
        [Reactive] public ReactiveObject SelectedViewModel { get; set; }
        
        public MainWindowViewModel()
        {
            string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudCam");
            _settingsSerializer = new SettingsSerializer(new FileInfo(Path.Combine(rootFolder, "settings.json")));
            _settings = LoadSettings(_settingsSerializer, rootFolder);

            _settingsViewModel = new SettingsViewModel(_settings, CameraDevicesEnumerator.GetAllConnectedCameras());
            _settingsViewModel.Apply.Subscribe(x =>
            {
                _settings = x;
                _settingsSerializer.Save(x);
            });
            _settingsViewModel.Start.Subscribe(x =>
            {
                _settings = x;
                _settingsSerializer.Save(x);
            });

            SelectedViewModel = _settingsViewModel;
        }

        private Settings LoadSettings(SettingsSerializer settingsSerializer, string rootFolder)
        {
            try
            {
                return settingsSerializer.Load();
            }
            catch (Exception)
            {
                // Prob. not yet initialized
                Settings settings = GetDefaultSettings(new DirectoryInfo(rootFolder));
                Directory.CreateDirectory(settings.MustacheFolder);
                Directory.CreateDirectory(settings.FrameFolder);
                Directory.CreateDirectory(settings.OutputFolder);
                return settings;
            }
        }

        private Settings GetDefaultSettings(DirectoryInfo rootFolder)
        {
            return new Settings(Path.Combine(rootFolder.FullName, "Frames"),
                Path.Combine(rootFolder.FullName, "Mustaches"),
                Path.Combine(rootFolder.FullName, "Output"),
                null);
        }
    }
}
