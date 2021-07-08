using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using CloudCam.Effect;
using CloudCam.Light;
using Light;
using OpenCvSharp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class MainWindowViewModel : ReactiveObject
    {
        private PhotoBoothViewModel _photoBoothViewModel;
        
        [Reactive]
        public Dictionary<Key, UserAction> KeyToUserActionDic { get; private set; }

        [Reactive] public ReactiveObject SelectedViewModel { get; set; }

        public ReactiveCommand<Key, UserAction> KeyPressed { get; }

        public MainWindowViewModel()
        {
            KeyPressed = ReactiveCommand.Create<Key, UserAction>((key) =>
            {
                if(KeyToUserActionDic != null && KeyToUserActionDic.TryGetValue(key, out UserAction value))
                {
                    return value;
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
                    case UserAction.ToggleDebugMode:
                        _photoBoothViewModel.DebugModeActive =! _photoBoothViewModel.DebugModeActive;
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

                KeyToUserActionDic = settings.KeyBindings.ToDictionary(y => y.Key, y => y.Action);

                var frameRepository = new ImageRepository(x.FrameFolder);
                frameRepository.Load();

                var mustachesRepository = new EffectImageLoader(new ImageRepository(x.MustacheFolder), new ImageSettingsRepository(x.MustacheFolder));
                mustachesRepository.Load();

                var hatsRepository = new EffectImageLoader(new ImageRepository(x.HatFolder), new ImageSettingsRepository(x.HatFolder));
                hatsRepository.Load();

                var glassesRepository = new EffectImageLoader(new ImageRepository(x.GlassesFolder), new ImageSettingsRepository(x.GlassesFolder));
                glassesRepository.Load();

                var outputRepository = new OutputImageRepository(x.OutputFolder);


#if DEBUG
                ILedAnimator ledAnimator = new NullLedAnimator();
#else
                var ledAnimator = new LedAnimator(250, 33, new LedController(250, 33, "COM12", 15));
                ledAnimator.StartAsync();
                ledAnimator.Animate();
#endif
                List<string> pickupLines = new List<string>
                {
                    "If you flash me then I’ll flash you",
                    "I've got you in my viewfinder",
                    "True love can never be photoshopped",
                    "Why don't you and I go into a dark room and see what develops?",
                    "When you flash your smile, my color temperature rises",
                    "I'm just a photo booth, but I can picture us together",
                    "Lets take it slow and see how things develop",
                    "That kiss was great! You're really upping my shutter speed",
                    "I only focus on you",
                    "That's not a telephoto lens in my pocket. I'm just happy to see you",
                    "I have to check if my camera is on auto focus because you are making everything else out-of-focus",
                    "I had to make my aperture smaller because you are gorgeously bright",
                    "Lets get our macro on and get in there nice and close",
                    "Come back to my place and I’ll shoot you with my Canon",
                    "I’m setting my focus on you",
                    "I left most of my gear at home but I did bring my 200mm",
                    "Was your daddy Ansel Adams? Because you’re a natural beauty",
                    "What say we go into a dark room and see what develops?",
                    "A portrait of you will need no photoshop at all",
                    "Before you were mine, everything was grayscale, but now I see the world in CMYK",
                    "Futura generations will speak of our romance",
                    "Hey girl you shine so bright I need to change my ISO to 100",
                    "I am a nudity photo booth, would you like to be my model for the night?",
                    "I like to be touched...and re-touched",
                    "I want to live life with you to the fullest resolution (300 dpi)",
                    "I wish I had an Eyedropper to capture the color of your eyes",
                    "I'll make your clothes 0% opacity",
                };


                _photoBoothViewModel = new PhotoBoothViewModel(CameraDevicesEnumerator.GetAllConnectedCameras().First(y => y.Name == x.CameraDevice),
                    frameRepository,
                    mustachesRepository,
                    glassesRepository,
                    glassesRepository,
                    outputRepository,
                    ledAnimator,
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
                Directory.CreateDirectory(settings.GlassesFolder);
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
                Path.Combine(rootFolder.FullName, "Glasses"),
                Path.Combine(rootFolder.FullName, "Output"),
                null,
                new KeyBindingSetting[]
                {
                    new KeyBindingSetting(UserAction.TakePicture, Key.Space),
                    new KeyBindingSetting(UserAction.MoveToPreviousFrame, Key.Left),
                    new KeyBindingSetting(UserAction.MoveToNextFrame, Key.Right),
                    new KeyBindingSetting(UserAction.MoveToPreviousEffect, Key.A),
                    new KeyBindingSetting(UserAction.MoveToNextEffect, Key.D),
                    new KeyBindingSetting(UserAction.ToggleDebugMode, Key.L),
                });
        }
    }
}
