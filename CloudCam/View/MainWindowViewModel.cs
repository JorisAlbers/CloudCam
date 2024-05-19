using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudCam.Effect;
using CloudCam.Light;
using CloudCam.Printing;
using Light;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam.View
{
    public class MainWindowViewModel : ReactiveObject
    {
        private PhotoBoothViewModel _photoBoothViewModel;
        
        [Reactive]
        public Dictionary<Key, UserAction> KeyToUserActionDic { get; private set; }

        [Reactive] public ReactiveObject SelectedViewModel { get; set; }

        [Reactive] public ErrorViewModel ErrorViewModel { get; set; }
     

        public ReactiveCommand<Key, UserAction> KeyPressed { get; }

        public MainWindowViewModel(ErrorObserver errorObserver)
        {
            Log.Logger.Information("Starting CloudCam");

            errorObserver
                .WhereNotNull()
                .Select(x=> new ErrorViewModel(x.RenderMessage()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ErrorViewModel = x);

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
                        await _photoBoothViewModel.Next.Execute(true);
                        break;
                    case UserAction.MoveToPreviousEffect:
                        await _photoBoothViewModel.Next.Execute(false);
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
                settingsSerializer.Save(x);
            });

            bool initializing = false;

            settingsViewModel.Start.Subscribe(async settingsToUse =>
            {
                if (initializing)
                {
                    return;
                }

                initializing = true;
                await InitializePhotoBoothViewModelAsync(settingsSerializer, settingsToUse);
                initializing = false;
            });

            SelectedViewModel = settingsViewModel;
        }

        private async Task InitializePhotoBoothViewModelAsync(SettingsSerializer settingsSerializer, Settings settings)
        {
            Log.Logger.Information("Initializing photo booth");
            settingsSerializer.Save(settings);

            try
            {
                _photoBoothViewModel = await InitializePhotoBoothViewModel(settings);
                SelectedViewModel = _photoBoothViewModel;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to initialize photo booth");
            }
        }

        private async Task<PhotoBoothViewModel> InitializePhotoBoothViewModel(Settings settings)
        {
            KeyToUserActionDic = settings.KeyBindings.ToDictionary(y => y.Key, y => y.Action);

            var frameRepository = new ImageRepository(settings.FrameFolder);
            frameRepository.Load();

            var mustachesRepository = new EffectImageLoader(new ImageRepository(settings.MustacheFolder),
                new ImageSettingsRepository(settings.MustacheFolder));
            mustachesRepository.Load();

            var hatsRepository = new EffectImageLoader(new ImageRepository(settings.HatFolder),
                new ImageSettingsRepository(settings.HatFolder));
            hatsRepository.Load();

            var glassesRepository = new EffectImageLoader(new ImageRepository(settings.GlassesFolder),
                new ImageSettingsRepository(settings.GlassesFolder));
            glassesRepository.Load();

            var outputRepository = new OutputImageRepository(settings.OutputFolder);

#if TRUE
            ILedAnimator ledAnimator = new NullLedAnimator();
#else
            var ledAnimator = new LedAnimator(33,216, new LedController (33,216, $"COM{settings.ComPortLeds}", 60));
            ledAnimator.StartAsync();
            ledAnimator.Animate();
#endif


            IPrinterManager printerManager = null;
            ImageCollageCreator imageCollageCreator = null;
            var shouldPrintImageViewModelFactory = new ElicitIfImageShouldBePrintedViewModelFactory(new Random(),
                _requestPrintMessages, _cancelPrintMessages, _okPrintMessages);
            try
            {
                printerManager = SetupForWhenAPrinterIsConnected(settings, out imageCollageCreator);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to setup the printer!");
            }

            var viewmodel = new PhotoBoothViewModel(
                settings.Theme,
                CameraDevicesEnumerator.GetAllConnectedCameras().First(y => y.Name == settings.CameraDevice),
                frameRepository,
                mustachesRepository,
                hatsRepository,
                glassesRepository,
                outputRepository,
                ledAnimator,
                GetPickupLines(),
                printerManager,
                imageCollageCreator,
                shouldPrintImageViewModelFactory);

            await viewmodel.Start();

            return viewmodel;
        }

        private static IPrinterManager SetupForWhenAPrinterIsConnected(Settings settings,
            out ImageCollageCreator imageCollageCreator)
        {
            IPrinterManager printerManager = null;
            imageCollageCreator = null;

            if (!String.IsNullOrWhiteSpace(settings.PrinterSettings.SelectedPrinter))
            {
#if DEBUG
                printerManager = new NullPrinterManager();
#else
                printerManager = new PrinterManager(settings.PrinterSettings.SelectedPrinter);
#endif
                var printerSpecs = printerManager.Initialize();

                // Total paper size is 2x6 inches wich is 600x1800 pixels
                int heightInPixels = 1800;
                int widthInPixels = 600;

                var imageSize = new Rectangle(0, 0, widthInPixels, heightInPixels);

                // Margins in between the images are fitted.
                double topMargin = 357;
                double leftMargin = 29;
                double rightMargin = 30;
                double bottomMargin = 475;
                int numberOfImages = 3;

                double widthPerImage = imageSize.Width - leftMargin - rightMargin; // Calculate width per image based on margins
                double heightPerImage = widthPerImage * (9.0 / 16.0); // To maintain 16:9 aspect ratio

                // Calculate margin between images. 
                double marginBetweenImages = (imageSize.Height - topMargin - bottomMargin - numberOfImages * heightPerImage) / (numberOfImages - 1);

                var rectangles = new Rectangle[numberOfImages];
                for (int i = 0; i < numberOfImages; i++)
                {
                    int x = (int)leftMargin;
                    int y = (int)(topMargin + i * heightPerImage + i * marginBetweenImages);

                    rectangles[i] = new Rectangle(x, y, (int)widthPerImage, (int)heightPerImage);
                }

                imageCollageCreator =
                    new ImageCollageCreator((Bitmap)Image.FromFile(settings.PrinterSettings.BackgroundImagePath), rectangles);
            }

            return printerManager;
        }

        private static List<string> GetPickupLines()
        {
            List<string> pickupLines = new List<string>
            {
/*                "If you flash me then I’ll flash you",
*/                "I've got you in my viewfinder",
                "True love can never be photoshopped",
                "Why don't you and I go into a dark room and see what develops?",
                "When you flash your smile, my color temperature rises",
                "I'm just a photo booth, but I can picture us together",
                "Lets take it slow and see how things develop",
/*                "That kiss was great! You're really upping my shutter speed",
*/                "I only focus on you",
/*                "That's not a telephoto lens in my pocket. I'm just happy to see you",
*/                "I have to check if my camera is on auto focus because you are making everything else out-of-focus",
                "I had to make my aperture smaller because you are gorgeously bright",
                "Lets get our macro on and get in there nice and close",
/*                "Come back to my place and I’ll shoot you with my Canon",
*/                "I’m setting my focus on you",
                "I left most of my gear at home but I did bring my 200mm",
/*                "Was your daddy Ansel Adams? Because you’re a natural beauty",
*/                "A portrait of you will need no photoshop at all",
                "Before you were mine, everything was grayscale, but now I see the world in CMYK",
                "Futura generations will speak of our romance",
                "Hey girl you shine so bright I need to change my ISO to 100",
/*                "I am a nudity photo booth, would you like to be my model for the night?",
*/                "I like to be touched...and re-touched",
                "I want to live life with you to the fullest resolution (300 dpi)",
                "I wish I had an Eyedropper to capture the color of your eyes",
/*                "I'll make your clothes 0% opacity",
*/                "Are you a memory card? Because my heart can't contain all the images of you.",
                "Are you a camera lens? Because every time I look at you, I focus.",
                "You must be a polaroid, because you make every moment instantaneously better.",
                "Do you believe in love at first sight, or should I take another picture of you?",
                "I must be a camera, because I can't seem to capture anyone's attention but yours.",
                "Are you a camera flash? Because you just illuminated my world.",
                "You're like a perfectly framed shot—picture-perfect from every angle.",
                "Are you a tripod? Because I want to stabilize our relationship.",
                "I'm like a zoom lens—I can't help but zoom in on you.",
                "Are you a photography studio? Because I want to capture our love in every setting.",
                "They say a picture is worth a thousand words, but with you, it's worth a million smiles.",
                "You're so picture-worthy, even National Geographic would want to feature you.",
                "If photography were illegal, you'd definitely be my partner in crime.",
                "I must be a darkroom, because I can't develop feelings for anyone but you.",
                "Are you a camera strap? Because you're the perfect accessory to my life.",
            };
            return pickupLines;
        }

        private List<string> _requestPrintMessages = new List<string>
            {
                "Press the red button and get ready for a steamy printout!",
                "Want some hot and spicy prints? Press the red button, baby!",
                "Press the red button to print out the sizzling moments captured!",
                "Get ready for a sexy surprise! Press the red button and watch it print!",
                "Press the red button and let the printer seductively print your image.",
                "Press the red button and get ready for a wild and steamy ride!",
                "Want to experience the pleasure of a hot and sensual printout? Press the red button now!",
                "Press the red button and let the printer seduce you with its tantalizing print quality.",
                "Press the red button and let the printer pleasure you with its smooth, sensual prints.",
                "Looking for a printout that'll make you weak in the knees? Press the red button and experience pure ecstasy!",
                "Press the red button and prepare to be seduced by the printer's passionate embrace.",
                "Want a printout that will make you blush with delight? Press the red button and let the printer unleash its sensuality!",
            };

        private List<string> _okPrintMessages = new List<string>()
        {
            "Printing!",
        };

        private List<string> _cancelPrintMessages = new List<string>()
        {
            "Sad!",
        };


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
                Theme.Cloud,
                new KeyBindingSetting[]
                {
                    new KeyBindingSetting(UserAction.TakePicture, Key.Space),
                    new KeyBindingSetting(UserAction.MoveToPreviousFrame, Key.Left),
                    new KeyBindingSetting(UserAction.MoveToNextFrame, Key.Right),
                    new KeyBindingSetting(UserAction.MoveToPreviousEffect, Key.A),
                    new KeyBindingSetting(UserAction.MoveToNextEffect, Key.D),
                    new KeyBindingSetting(UserAction.ToggleDebugMode, Key.L),
                },
                13,
                new PrinterSettings(null, null));
        }
    }
}
