using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CloudCam.Effect;
using CloudCam.Light;
using CloudCam.Printing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Color = System.Windows.Media.Color;
using Point = System.Drawing.Point;

namespace CloudCam.View
{
    public class PhotoBoothViewModel : ReactiveObject
    {
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

        [Reactive] private WebcamCapture Capture { get; set; }
        [Reactive] private ImageTransformer ImageTransformer { get; set; }
        [Reactive] private ImageToDisplayImageConverter ImageToDisplayImageConverter { get; set; }

        [Reactive] public int SecondsUntilPictureIsTaken { get; set; } = -1;

        [Reactive] public string PickupLine { get; set; }

        [Reactive] public ImageSource TakenImage { get; set; }

        [ObservableAsProperty]
        public ImageSourceWithMat ImageSource { get; }

        [ObservableAsProperty]
        public ImageSourceWithMat Frame { get; }

        [ObservableAsProperty]
        public float CameraFps { get; set; }  
        
        [ObservableAsProperty]
        public float ToDisplayImageFps { get; set; }

        [ObservableAsProperty]
        public float EditingFps { get; set; }

        [Reactive] public ElicitIfImageShouldBePrintedViewModel ElicitIfImageShouldBePrintedViewModel { get; set; }

        [Reactive] public bool DebugModeActive { get; set; }

        public ReactiveCommand<bool, ImageSourceWithMat> NextFrame { get; }
        public ReactiveCommand<bool, IEffect> NextEffect { get; }

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

            _frameManager = new FrameManager(frameRepository);
            NextFrame = ReactiveCommand.CreateFromTask<bool, ImageSourceWithMat>(LoadNextFrameAsync);
            NextFrame.WhereNotNull().ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

           
            EffectManager effectManager = new EffectManager(@"Resources\Caff\deploy.prototxt", @"Resources\Caff\res10_300x300_ssd_iter_140000_fp16.caffemodel",  @"Resources\Cascades\haarcascade_mcs_nose.xml" , @"Resources\Cascades\haarcascade_eye_tree_eyeglasses.xml", mustachesRepository, hatsRepository, glassesRepository);
            NextEffect = ReactiveCommand.Create<bool, IEffect>((forwards) =>
            {
                var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
                if (elicitViewModel != null)
                {
                    elicitViewModel.Cancel();
                    return null;
                }

                if (forwards)
                {
                    return effectManager.Next();
                }

                return effectManager.Previous();
            });
            _transformationSettings = new TransformationSettings();
            NextEffect.WhereNotNull().Subscribe(x => _transformationSettings.Effect = x);

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Unit>(TakePictureAsync);
            
            ImageSourceWithMat previous = null;
            Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Select(_ => ImageToDisplayImageConverter?.ImageSourceWithMat)
                .WhereNotNull()
                .Where(x=> previous != x)
                .Do(x => previous = x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x=>x.ImageSource);


            this.WhenAnyValue(x => x.Capture.Fps)
                .Where((_)=> DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.CameraFps);

            this.WhenAnyValue(x => x.ImageTransformer.Fps)
                .Where((_) => DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.EditingFps);

            this.WhenAnyValue(x => x.ImageToDisplayImageConverter.Fps)
                .Where((_) => DebugModeActive)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ToDisplayImageFps);
        }

        public async Task Start()
        {
            MatBuffer matBuffer = new MatBuffer();
            WebcamCapture webcamCapture;

            try
            {
                webcamCapture = new WebcamCapture(_device.OpenCdId, matBuffer);
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
                
                ImageTransformer = new ImageTransformer(matBuffer);
                Task t2 =  ImageTransformer.StartAsync(_transformationSettings, cancellationTokenSource.Token);

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
                    NextEffect.Execute(true).Subscribe();
                    break;
                case SwipeDirection.Right:
                    NextEffect.Execute(false).Subscribe();
                    break;
                case SwipeDirection.Up:
                /*
                    await TakePicture.Execute();
                    */
                    break;
                case SwipeDirection.Down:
                    /*await this.ResetFramesAndEffects();*/
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            await Task.CompletedTask;
        }

        private async Task<Unit> TakePictureAsync(Unit unit, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _takingPicture, 1) == 1)
            {
                var elicitViewModel = ElicitIfImageShouldBePrintedViewModel;
                if (elicitViewModel != null)
                {
                    elicitViewModel.Accept();
                }

                return Unit.Default;
            }

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
                 Interlocked.Exchange(ref _takingPicture, 0);
             }


            return Unit.Default;
        }

        private void ResetPhotoBooth()
        {
            SecondsUntilPictureIsTaken = -1;
            TakenImage = null;
            PickupLine = null;
        }


        private async Task TakeOnePicture(CancellationToken cancellationToken)
        {
            var imageAsBitmap = await TakeImage(cancellationToken);
            TakenImage = imageAsBitmap.ToBitmapSource();
            SecondsUntilPictureIsTaken = -1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await Task.Delay(100, cancellationToken); // allow gui to update
            _outputImageRepository.Save(imageAsBitmap);
            await Task.Delay(3000, cancellationToken);
            TakenImage = null;
            PickupLine = null;
        }

        
        private async Task TakeThreePicturesOnBackground(CancellationToken cancellationToken)
        {
            // image 1
            var imageAsBitmap1 = await TakeImage(cancellationToken);
            var imageAsImageSource1 = imageAsBitmap1.ToBitmapSource();
            _outputImageRepository.Save(imageAsBitmap1);
            TakenImage = imageAsImageSource1;
            SecondsUntilPictureIsTaken = -1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];

            if (!await ShouldPrintImage(_elicitShouldPrintViewModelFactory))
            {
                Log.Logger.Information("User does not want to print the image");
                TakenImage = null;
                PickupLine = null;
                ElicitIfImageShouldBePrintedViewModel = null;
                return;
            }
            Log.Logger.Information("User requested to print the image");

            // reset
            ElicitIfImageShouldBePrintedViewModel = null;
            TakenImage = null;
            PickupLine = $"2 to go!";
            await Task.Delay(2000, cancellationToken);
            PickupLine = null;


            // image 2
            var imageAsBitmap2 = await TakeImage(cancellationToken);
            var imageAsImageSource2 = imageAsBitmap2.ToBitmapSource();
            _outputImageRepository.Save(imageAsBitmap2);
            TakenImage = imageAsImageSource2;
            SecondsUntilPictureIsTaken = -1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await Task.Delay(1000, cancellationToken); // allow gui to update
            // reset
            TakenImage = null;
            PickupLine = $"1 to go!";
            await Task.Delay(2000, cancellationToken);
            PickupLine = null;

            // image 3
            var imageAsBitmap3 = await TakeImage(cancellationToken);
            var imageAsImageSource3 = imageAsBitmap3.ToBitmapSource();
            _outputImageRepository.Save(imageAsBitmap3);
            TakenImage = imageAsImageSource3;

            // print
            Bitmap toPrint = await _imageCollageCreator.Create(new Bitmap[] { imageAsBitmap1, imageAsBitmap2, imageAsBitmap3 }, cancellationToken);
            _printerManager.Print(toPrint);
            _outputImageRepository.Save(toPrint);

            SecondsUntilPictureIsTaken = -1;
            TakenImage = imageAsImageSource3;
            await Task.Delay(1000, cancellationToken);
            TakenImage = imageAsImageSource2;
            await Task.Delay(1000, cancellationToken);
            TakenImage = imageAsImageSource1;
            await Task.Delay(1000, cancellationToken);
            TakenImage = null;
        }

        private async Task<bool> ShouldPrintImage(ElicitIfImageShouldBePrintedViewModelFactory elicitIfImageShouldBePrintedViewModelFactory)
        {
            var viewmodel = elicitIfImageShouldBePrintedViewModelFactory.Create();
            ElicitIfImageShouldBePrintedViewModel = viewmodel;
            await Task.Delay(100); // allow GUI to update
            bool shouldPrint = await viewmodel.Start();
            await Task.Delay(1000);
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

            return await Task.Run(() =>
            {
                if (forwards)
                {
                    return _frameManager.Next(Capture.FrameSize);
                }

                return _frameManager.Previous(Capture.FrameSize);
            });
        }

        private async Task<Bitmap> TakeImage(CancellationToken cancellationToken)
        {
            Log.Logger.Information("Capturing an image");
            _ledAnimator.StartFlash();

            SecondsUntilPictureIsTaken = 3;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 2;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 1;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 0;
            await Task.Delay(500, cancellationToken); // allow camera to adjust to the flash

            var imageAsBitmap = ImageSource.Mat.ToBitmap();
            if (Frame?.Mat != null)
            {
                using Bitmap frameAsBitmap = Frame.Mat.ToBitmap();
                await Task.Run(() =>
                {
                    // Overlay frame on top of image
                    using Graphics gr = Graphics.FromImage(imageAsBitmap);
                    gr.DrawImage(frameAsBitmap, new System.Drawing.Point(0, 0));
                }, cancellationToken);
            }
            _ledAnimator.EndFlash();
            return imageAsBitmap;
        }
    }

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