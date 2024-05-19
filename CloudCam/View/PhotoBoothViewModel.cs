using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CloudCam.Effect;
using CloudCam.Light;
using CloudCam.Printing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam.View
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private const int _SECONDS_IMAGE_DISPLAYED = 3;
        private const int _SECONDS_COUNTDOWN_BEFORE_IMAGE_TAKEN = 5;
        private readonly CameraDevice _device;
        private readonly OutputImageRepository _outputImageRepository;
        private readonly ILedAnimator _ledAnimator;
        private readonly List<string> _pickupLines;
        private readonly IPrinterManager _printerManager;
        private readonly ImageCollageCreator _imageCollageCreator;
        private readonly ElicitIfImageShouldBePrintedViewModelFactory _elicitShouldPrintViewModelFactory;
        private readonly FrameManager _frameManager;
        private readonly Random _random;
        private int _takingPicture;
        private readonly TransformationSettings _transformationSettings;
        private readonly EffectManager _effectManager;
        private GalleryViewModel _galleryViewModel;

        [Reactive] private WebcamCapture Capture { get; set; }
        [Reactive] private ForegroundLocator ForegroundLocator { get; set; }
        [Reactive] private ImageToDisplayImageConverter ImageToDisplayImageConverter { get; set; }

        [Reactive] public int SecondsUntilPictureIsTaken { get; set; } = -1;

        [Reactive] public string PickupLine { get; set; }

        [Reactive] public string PhotoCountdownText { get; set; }

        [Reactive] public ImageSource TakenImage { get; set; }

        [Reactive] public ImageSource MultipleTakenImage1 { get; set; }
        [Reactive] public ImageSource MultipleTakenImage2 { get; set; }
        [Reactive] public ImageSource MultipleTakenImage3 { get; set; }

        [ObservableAsProperty]
        public ImageSourceWithMat ImageSource { get; }

        [Reactive] public List<ForegroundImage> ForegroundImages { get; set; }

        [Reactive] public GalleryViewModel GalleryViewModel { get; set; }

        [Reactive]
        public ImageSourceWithMat Frame { get; set; }

        [ObservableAsProperty]
        public float CameraFps { get; set; }  
        
        [ObservableAsProperty]
        public float ToDisplayImageFps { get; set; }

        [ObservableAsProperty]
        public float EditingFps { get; set; }

        [Reactive] public ElicitIfImageShouldBePrintedViewModel ElicitIfImageShouldBePrintedViewModel { get; set; }
        [Reactive] public PrintingViewModel PrintingViewModel { get; set; }

        [Reactive] public bool DebugModeActive { get; set; }

        public ReactiveCommand<bool, ImageSourceWithMat> NextFrame { get; }

        public ReactiveCommand<bool, Unit> Next { get; }
        public ReactiveCommand<Unit, Unit> ClearFramesAndEffects { get; }
        
        public ReactiveCommand<Unit,Unit> TakePicture { get; }

        [Reactive] public PictureMode PictureMode { get; set; } = PictureMode.ThreeOnBackground;

        
        public PhotoBoothViewModel(CameraDevice device, 
            ImageRepository frameRepository, 
            EffectImageLoader mustachesRepository, 
            EffectImageLoader hatsRepository,
            EffectImageLoader glassesRepository,
            OutputImageRepository outputImageRepository, 
            ILedAnimator ledAnimator, 
            List<string> pickupLines,
            IPrinterManager printerManager,
            ImageCollageCreator imageCollageCreator,
            ElicitIfImageShouldBePrintedViewModelFactory elicitShouldPrintViewModelFactory)
        {
            Log.Logger.Information("Starting photo booth");
            _device = device;
            _outputImageRepository = outputImageRepository;
            _ledAnimator = ledAnimator;
            _pickupLines = pickupLines;
            _printerManager = printerManager;
            _imageCollageCreator = imageCollageCreator;
            _elicitShouldPrintViewModelFactory = elicitShouldPrintViewModelFactory;
            _random = new Random();
            _galleryViewModel = new GalleryViewModel(outputImageRepository, 5, _random);

            _frameManager = new FrameManager(frameRepository);
            // Next frame
            NextFrame = ReactiveCommand.CreateFromTask<bool, ImageSourceWithMat>(LoadNextFrameAsync);
            NextFrame.WhereNotNull().ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => Frame = x);
            // Clear frames and effect
            ClearFramesAndEffects = ReactiveCommand.Create(InternalClearFramesAndEffects);
            ClearFramesAndEffects.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x =>
            {
                Frame = new ImageSourceWithMat(null, null);
                if (_transformationSettings != null) _transformationSettings.Effect = null;
            });

           
            _effectManager = new EffectManager(@"Resources\Caff\deploy.prototxt", @"Resources\Caff\res10_300x300_ssd_iter_140000_fp16.caffemodel",  @"Resources\Cascades\haarcascade_mcs_nose.xml" , @"Resources\Cascades\haarcascade_eye_tree_eyeglasses.xml", mustachesRepository, hatsRepository, glassesRepository);

            Next = ReactiveCommand.Create<bool, Unit>((forwards =>
            {
                var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
                if (elicitViewModel != null)
                {
                    elicitViewModel.Cancel();
                    return Unit.Default;
                }

                LoadNextEffect(forwards);
                return Unit.Default;
            }));
            
            
            _transformationSettings = new TransformationSettings();

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Unit>(TakePictureAsync);
            
            ImageSourceWithMat previous = null;
            Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Select(_ => ImageToDisplayImageConverter?.ImageSourceWithMat)
                .WhereNotNull()
                .Where(x=> previous != x)
                .Do(x => previous = x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(x=> ForegroundImages =  _transformationSettings.CurrentForegrounds)// TODO this is not sync. The foregroundimages need to be bundled with the frame.
                .ToPropertyEx(this, x=>x.ImageSource);


            this.WhenAnyValue(x => x.Capture.Fps)
                .Where((_)=> DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.CameraFps);

            this.WhenAnyValue(x => x.ForegroundLocator.Fps)
                .Where((_) => DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.EditingFps);

            this.WhenAnyValue(x => x.ImageToDisplayImageConverter.Fps)
                .Where((_) => DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ToDisplayImageFps);

            if (_printerManager != null)
            {
                this.WhenAnyValue(x => x._printerManager.IsPrinting)
                    .Where(x => !x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        PrintingViewModel = null;
                    });
            }
        }

        private Unit InternalClearFramesAndEffects()
        {
            var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
            if (elicitViewModel != null)
            {
                elicitViewModel.Cancel();
            }

            if (SecondsUntilPictureIsTaken > 0)
            {
                SecondsUntilPictureIsTaken = _SECONDS_COUNTDOWN_BEFORE_IMAGE_TAKEN;
            }

            return Unit.Default;
        }

        private void LoadNextEffect(bool forwards)
        {
            if (SecondsUntilPictureIsTaken > 0)
            {
                SecondsUntilPictureIsTaken = _SECONDS_COUNTDOWN_BEFORE_IMAGE_TAKEN;
            }

            if (forwards)
            {
                _transformationSettings.Effect =  _effectManager.Next();
                return;
            }

            _transformationSettings.Effect = _effectManager.Previous();
        }

        public async Task Start()
        {
            MatBuffer matBuffer = new MatBuffer();
            WebcamCapture webcamCapture;

            try
            {
                webcamCapture = new WebcamCapture(_device, matBuffer);
                await webcamCapture.Initialize();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to setup webcam capture!");
                throw;
            }

            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task t =  webcamCapture.CaptureAsync(cancellationTokenSource.Token);
                Capture = webcamCapture;
                
                ForegroundLocator = new ForegroundLocator(matBuffer);
                Task t2 =  ForegroundLocator.StartAsync(_transformationSettings, cancellationTokenSource.Token);

                ImageToDisplayImageConverter = new ImageToDisplayImageConverter(matBuffer);
                Task t3 =  ImageToDisplayImageConverter.StartAsync(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex,"Failed to setup image processing pipeline");
                throw;
            }

        }

        public async Task HandelSwipeInput(SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.Left:
                    Next.Execute(true).Subscribe();
                    break;
                case SwipeDirection.Right:
                    Next.Execute(false).Subscribe();
                    break;
                case SwipeDirection.Up:
                /*
                    await TakePicture.Execute();
                    */
                    break;
                case SwipeDirection.Down:
                    ClearFramesAndEffects.Execute().Subscribe();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            await Task.CompletedTask;
        }

        private async Task<Unit> TakePictureAsync(Unit unit, CancellationToken cancellationToken)
        {
            // If we are questioning the user if the image should be printed, we don't want to take another one
            var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
            if (elicitViewModel != null)
            {
                elicitViewModel.Accept();
                return Unit.Default;
            }

            // If we are already taking a picture, we don't want to take another one
            if (Interlocked.Exchange(ref _takingPicture, 1) == 1)
            {
                return Unit.Default;
            }

            try
            {
                // If we are printing, we don't want to take another one
                if (_printerManager != null && _printerManager.IsPrinting)
                {
                    return Unit.Default;
                }
                // If we are in the middle of a countdown, we don't want to take another one
                if (SecondsUntilPictureIsTaken > 0)
                {
                    return Unit.Default;
                }

                await InternalTakePictureAsync(cancellationToken);

            }
            finally
            {
                Interlocked.Exchange(ref _takingPicture, 0);
            }
            
            return Unit.Default;
        }

        private async Task InternalTakePictureAsync(CancellationToken cancellationToken)
        {
            Log.Logger.Information("Taking a picture with mode {PictureMode}", PictureMode);

            try
            {
                switch (PictureMode)
                {
                    case PictureMode.OneAtATime:
                        await TakeOnePicture(cancellationToken);
                        break;
                    case PictureMode.ThreeOnBackground:
                        await TakeThreePicturesOnBackground(cancellationToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to take an image!");
            }
            finally
            {
                ResetPhotoBooth();
            }
        }

        private void ResetPhotoBooth()
        {
            SecondsUntilPictureIsTaken = -1;
            TakenImage = null;
            PickupLine = null;
        }


        private async Task TakeOnePicture(CancellationToken cancellationToken)
        {
            var imageAsBitmap = await TakeImage(cancellationToken, 0);
            TakenImage = imageAsBitmap.ToBitmapSource();
            SecondsUntilPictureIsTaken = -1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await Task.Delay(100, cancellationToken); // allow gui to update
            _outputImageRepository.Save(imageAsBitmap);
            await AllowUserToLookAtImage(cancellationToken);
            TakenImage = null;
            PickupLine = null;
        }


        private async Task TakeThreePicturesOnBackground(CancellationToken cancellationToken)
        {
            // image 1
            var imageAsBitmap1 = await TakeImage(cancellationToken, 3);
            var imageAsImageSource1 = imageAsBitmap1.ToBitmapSource();
            TakenImage = imageAsImageSource1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await AllowUserToLookAtImage(cancellationToken);
            TakenImage = null;
            PickupLine = null;

            // image 2
            var imageAsBitmap2 = await TakeImage(cancellationToken, 2);
            var imageAsImageSource2 = imageAsBitmap2.ToBitmapSource();
            TakenImage = imageAsImageSource2;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await AllowUserToLookAtImage(cancellationToken);
            TakenImage = null;
            PickupLine = null;

            // image 3
            var imageAsBitmap3 = await TakeImage(cancellationToken, 1);
            var imageAsImageSource3 = imageAsBitmap3.ToBitmapSource();
            TakenImage = imageAsImageSource3;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            // create collage while user is looking at image
            Bitmap collage = await CreateCollageAsync(imageAsBitmap1, imageAsBitmap2, imageAsBitmap3, cancellationToken);
            await AllowUserToLookAtImage(cancellationToken); // TODO combine with wait time for the collage creation.
            TakenImage = null;
            PickupLine = null;

            // Show the collage
            // TODO instead of showing the end result, show the three images enlarged next to each other.
            BitmapSource collageAsBitmapSource = collage.ToBitmapSource();
            MultipleTakenImage1 = imageAsImageSource1;
            MultipleTakenImage2 = imageAsImageSource2;
            MultipleTakenImage3 = imageAsImageSource3;

            if (!await ShouldPrintImage(_elicitShouldPrintViewModelFactory))
            {
                Log.Logger.Information("User does not want to print the image");
                TakenImage = null;
                PickupLine = null;
                SecondsUntilPictureIsTaken = -1;
                ElicitIfImageShouldBePrintedViewModel = null;
                MultipleTakenImage1 = null;
                MultipleTakenImage2 = null;
                MultipleTakenImage3 = null;
                return;
            }

            Log.Logger.Information("User requested a print of the image");
            _printerManager.Print(collage);
            _outputImageRepository.Save(collage);

            SecondsUntilPictureIsTaken = -1;
            TakenImage = null;
            PickupLine = null;
            ElicitIfImageShouldBePrintedViewModel = null;
            MultipleTakenImage1 = null;
            MultipleTakenImage2 = null;
            MultipleTakenImage3 = null;

            PrintingViewModel = new PrintingViewModel(collageAsBitmapSource);
        }

        private async Task AllowUserToLookAtImage(CancellationToken cancellationToken)
        {
            await Task.Delay(_SECONDS_IMAGE_DISPLAYED * 1000, cancellationToken);
        }


        private async Task<Bitmap> CreateCollageAsync(Bitmap imageAsBitmap1, Bitmap imageAsBitmap2, Bitmap imageAsBitmap3, CancellationToken cancellationToken)
        {
            return await _imageCollageCreator.Create(new Bitmap[] { imageAsBitmap1, imageAsBitmap2, imageAsBitmap3 }, PickupLine, cancellationToken);
        }

        private async Task<bool> ShouldPrintImage(ElicitIfImageShouldBePrintedViewModelFactory elicitIfImageShouldBePrintedViewModelFactory)
        {
            var viewmodel = elicitIfImageShouldBePrintedViewModelFactory.Create();
            ElicitIfImageShouldBePrintedViewModel = viewmodel;
            await Task.Delay(100); // allow GUI to update
            bool shouldPrint = await viewmodel.Start();
            return shouldPrint;
        }

        private async Task<ImageSourceWithMat> LoadNextFrameAsync(bool forwards)
        {
            var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
            if (elicitViewModel != null)
            {
                elicitViewModel.Cancel();
                return null;
            }

            if (SecondsUntilPictureIsTaken > 0)
            {
                SecondsUntilPictureIsTaken = _SECONDS_COUNTDOWN_BEFORE_IMAGE_TAKEN;
            }


            return await Task.Run(() =>
            {
                if (forwards)
                {
                    return _frameManager.Next(Capture.FrameSize);
                }

                return _frameManager.Previous(Capture.FrameSize);
            });
        }

        private async Task<Bitmap> TakeImage(CancellationToken cancellationToken, int numberOfPicture)
        {
            if (numberOfPicture != 0)
            {
                PhotoCountdownText = $"{numberOfPicture} to go!";
            }    
            Log.Logger.Information("Capturing an image");

            SecondsUntilPictureIsTaken = _SECONDS_COUNTDOWN_BEFORE_IMAGE_TAKEN;
            // SecondsUntilPictureIsTaken might be modified during this loop.
            while (SecondsUntilPictureIsTaken > 0)
            {
                await Task.Delay(1000, cancellationToken);
                SecondsUntilPictureIsTaken--;
            }

            _ledAnimator.StartFlash();
            await Task.Delay(500, cancellationToken); // allow camera to adjust to the flash

            var imageAsBitmap = ImageSource.Mat.ToBitmap();
            var foregrounds   = ForegroundImages;


            if (Frame?.Mat != null)
            {
                using Bitmap frameAsBitmap = Frame.Mat.ToBitmap();
                await Task.Run(() =>
                {
                    // Overlay frame on top of image
                    using Graphics gr = Graphics.FromImage(imageAsBitmap);

                    if(foregrounds != null)
                    {
                        // Draw foregrounds (the hats and mustaches and stuff)
                        foreach (ForegroundImage foregroundImage in foregrounds)
                        {
                            var resized = foregroundImage.image.Resize(foregroundImage.rect.Size);
                            var bitmap = resized.ToBitmap();

                            gr.DrawImage(bitmap, new System.Drawing.Point(foregroundImage.rect.X, foregroundImage.rect.Y));
                        }
                    }             
                  
                    
                    // Draw frame
                    gr.DrawImage(frameAsBitmap, new System.Drawing.Point(0, 0));
                    
                }, cancellationToken);
            }
            _ledAnimator.EndFlash();
            PhotoCountdownText = null;
            SecondsUntilPictureIsTaken = -1;
            await Task.Delay(50, cancellationToken);
            _outputImageRepository.Save(imageAsBitmap);
            return imageAsBitmap;
        }
    }

    public record ForegroundImage(Mat image, Rect rect);

    public class ImageSourceWithMat
    {
        public ImageSource ImageSource { get; }
        public Mat Mat { get; }

        public ImageSourceWithMat(ImageSource imageSource, Mat mat)
        {
            ImageSource = imageSource;
            Mat = mat;
        }
    }

    public enum PictureMode
    {
        OneAtATime,
        ThreeOnBackground,
    }

}