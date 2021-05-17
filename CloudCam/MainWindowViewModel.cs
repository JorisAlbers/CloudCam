using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using OpenCvSharp;
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

        public ReactiveCommand<Key, UserAction> KeyPressed { get; }

        public MainWindowViewModel()
        {
            KeyPressed = ReactiveCommand.Create<Key, UserAction>((key) =>
            {
                switch (key)
                {
                    case Key.Right: return UserAction.MoveToNextFrame;
                    case Key.Left:  return UserAction.MoveToPreviousFrame;
                    case Key.Space:  return UserAction.TakePicture;
                }

                return UserAction.None;
            });

            KeyPressed.Where(x => x != UserAction.None).Subscribe(async x =>
            {
                if (_photoBoothViewModel == null)
                {
                    return;
                }

                if (x == UserAction.MoveToNextFrame)
                {
                    await _photoBoothViewModel.NextFrame.Execute(true);
                }
                else if (x == UserAction.MoveToPreviousFrame)
                {
                    await _photoBoothViewModel.NextFrame.Execute(false);
                }
                else if (x == UserAction.TakePicture)
                {
                    await _photoBoothViewModel.TakePicture.Execute();
                }
            });
            
            string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudCam");
            _settingsSerializer = new SettingsSerializer(new FileInfo(Path.Combine(rootFolder, "settings.json")));
            _settings = LoadSettings(_settingsSerializer, rootFolder);

            var devices = CameraDevicesEnumerator.GetAllConnectedCameras();
            _settingsViewModel = new SettingsViewModel(_settings, devices);
            _settingsViewModel.Apply.Subscribe(x =>
            {
                _settings = x;
                _settingsSerializer.Save(x);
            });
            _settingsViewModel.Start.Subscribe(x =>
            {
                _settings = x;
                _settingsSerializer.Save(x);

                var frameRepository = new ImageRepository(x.FrameFolder);
                frameRepository.Load();

                var outputRepository = new OutputImageRepository(x.OutputFolder);


                _photoBoothViewModel = new PhotoBoothViewModel(x,
                    CameraDevicesEnumerator.GetAllConnectedCameras().First(y => y.Name == x.CameraDevice),
                    frameRepository,
                    outputRepository);
                SelectedViewModel = _photoBoothViewModel;
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

    public class OutputImageRepository
    {
        private readonly string _folder;

        public OutputImageRepository(string folder)
        {
            _folder = folder;
        }

        public void Save(Bitmap image)
        {
            DateTime now = DateTime.Now;

            string fileName = $"CloudCam_{now:yyyy-M-dd--H-mm-ss}.jpg";
            image.Save(Path.Combine(_folder, fileName));
        }
    }

    public enum UserAction
    {
        None,
        MoveToNextFrame,
        MoveToPreviousFrame,
        TakePicture,
    }
}
