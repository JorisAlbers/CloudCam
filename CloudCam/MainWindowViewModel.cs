using System;
using System.Collections.Generic;
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
        private PhotoBoothViewModel _photoBoothViewModel;

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
                    case Key.D: return UserAction.MoveToNextEffect;
                    case Key.A:  return UserAction.MoveToPreviousEffect;
                }

                return UserAction.None;
            });

            KeyPressed.Where(x => x != UserAction.None).Subscribe(async x =>
            {
                if (_photoBoothViewModel == null)
                {
                    return;
                }

                switch (x)
                {
                    case UserAction.MoveToNextFrame:
                        await _photoBoothViewModel.NextFrame.Execute(true);
                        break;
                    case UserAction.MoveToPreviousFrame:
                        await _photoBoothViewModel.NextFrame.Execute(false);
                        break;
                    case UserAction.TakePicture:
                        await _photoBoothViewModel.TakePicture.Execute();
                        break;
                    case UserAction.MoveToNextEffect:
                        await _photoBoothViewModel.NextEffect.Execute(true);
                        break;
                    case UserAction.MoveToPreviousEffect:
                        await _photoBoothViewModel.NextEffect.Execute(false);
                        break;
                }
                
            });
            
            string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CloudCam");
            var settingsSerializer = new SettingsSerializer(new FileInfo(Path.Combine(rootFolder, "settings.json")));
            var settings = LoadSettings(settingsSerializer, rootFolder);

            var devices = CameraDevicesEnumerator.GetAllConnectedCameras();
            var settingsViewModel = new SettingsViewModel(settings, devices);
            settingsViewModel.Apply.Subscribe(x =>
            {
                settings = x;
                settingsSerializer.Save(x);
            });
            settingsViewModel.Start.Subscribe(x =>
            {
                settings = x;
                settingsSerializer.Save(x);

                var frameRepository = new ImageRepository(x.FrameFolder);
                frameRepository.Load();

                var mustachesRepository = new ImageRepository(x.MustacheFolder);
                mustachesRepository.Load();

                var hatsRepository = new ImageRepository(x.HatFolder);
                hatsRepository.Load();

                var outputRepository = new OutputImageRepository(x.OutputFolder);

                List<string> pickupLines = new List<string>
                {
                    "If you flash me then I’ll flash you",
                    "I've got you in my viewfinder.",
                    "True love can never be photoshopped.",
                    "Why don't you and I go into a dark room and see what develops?",
                    "When you flash your smile, my color temperature rises.",
                    "I'm just a photo booth, but I can picture us together.",
                    "Lets take it slow and see how things develop.",
                    "That kiss was great! You're really upping my shutter speed. ",
                    "I only focus on you.",
                    "That's not a telephoto lens in my pocket. I'm just happy to see you.",
                };


                _photoBoothViewModel = new PhotoBoothViewModel(CameraDevicesEnumerator.GetAllConnectedCameras().First(y => y.Name == x.CameraDevice),
                    frameRepository,
                    mustachesRepository,
                    hatsRepository,
                    outputRepository,
                    pickupLines
                    );
                SelectedViewModel = _photoBoothViewModel;
            });

            SelectedViewModel = settingsViewModel;
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
                Directory.CreateDirectory(settings.HatFolder);
                Directory.CreateDirectory(settings.FrameFolder);
                Directory.CreateDirectory(settings.OutputFolder);
                return settings;
            }
        }

        private Settings GetDefaultSettings(DirectoryInfo rootFolder)
        {
            return new Settings(Path.Combine(rootFolder.FullName, "Frames"),
                Path.Combine(rootFolder.FullName, "Mustaches"),
                Path.Combine(rootFolder.FullName, "Hats"),
                Path.Combine(rootFolder.FullName, "Output"),
                null);
        }
    }
}
