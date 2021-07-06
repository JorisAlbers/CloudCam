using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using CloudCam.Effect;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Color = System.Windows.Media.Color;

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly OutputImageRepository _outputImageRepository;
        private readonly List<string> _pickupLines;
        private readonly WebcamCapture _capture;
        private readonly ImageTransformer _imageTransformer;
        private readonly ImageToDisplayImageConverter _imageToDisplayImageConverter;
        private readonly FrameManager _frameManager;
        private readonly Random _random;

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

        [Reactive] public bool DebugModeActive { get; set; }

        public ReactiveCommand<bool, ImageSourceWithMat> NextFrame { get; }
        public ReactiveCommand<bool, IEffect> NextEffect { get; }

        public ReactiveCommand<Unit,Unit> TakePicture { get; }

        

        public PhotoBoothViewModel(CameraDevice device, ImageRepository frameRepository, EffectImageLoader mustachesRepository, EffectImageLoader hatsRepository,
            OutputImageRepository outputImageRepository, List<string> pickupLines)
        {
            _outputImageRepository = outputImageRepository;
            _pickupLines = pickupLines;
            _random = new Random();
            MatBuffer matBuffer = new MatBuffer();

            _frameManager = new FrameManager(frameRepository);
            NextFrame = ReactiveCommand.CreateFromTask<bool, ImageSourceWithMat>(LoadNextFrameAsync);
            NextFrame.ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

            TransformationSettings transformationSettings = new TransformationSettings();
            EffectManager effectManager = new EffectManager(@"Resources\Caff\deploy.prototxt", @"Resources\Caff\res10_300x300_ssd_iter_140000_fp16.caffemodel",  @"Resources\Cascades\haarcascade_mcs_nose.xml" , @"Resources\Cascades\haarcascade_eye_tree_eyeglasses.xml", mustachesRepository, hatsRepository);
            NextEffect = ReactiveCommand.Create<bool, IEffect>((forwards) =>
            {
                if (forwards)
                {
                    return effectManager.Next();
                }

                return effectManager.Previous();
            });
            NextEffect.Subscribe(x => transformationSettings.Effect = x);

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Unit>(TakePictureAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            _capture = new WebcamCapture(device.OpenCdId, matBuffer);
            _ = _capture.CaptureAsync(cancellationTokenSource.Token);

            _imageTransformer = new ImageTransformer(matBuffer);
            Task t1 = _imageTransformer.StartAsync(transformationSettings, cancellationTokenSource.Token);

            _imageToDisplayImageConverter = new ImageToDisplayImageConverter(matBuffer);
            Task t2 = _imageToDisplayImageConverter.StartAsync(cancellationTokenSource.Token);

            ImageSourceWithMat previous = null;
            Observable.Interval(TimeSpan.FromMilliseconds(33))
                .Select(_ => _imageToDisplayImageConverter.ImageSourceWithMat)
                .WhereNotNull()
                .Where(x=> previous != x)
                .Do(x => previous = x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x=>x.ImageSource);


            this.WhenAnyValue(x => x._capture.Fps).Where((_)=> DebugModeActive).ObserveOn(RxApp.MainThreadScheduler).ToPropertyEx(this, x => x.CameraFps);
            this.WhenAnyValue(x => x._imageTransformer.Fps).Where((_) => DebugModeActive).ObserveOn(RxApp.MainThreadScheduler).ToPropertyEx(this, x => x.EditingFps);
            this.WhenAnyValue(x => x._imageToDisplayImageConverter.Fps).Where((_) => DebugModeActive).ObserveOn(RxApp.MainThreadScheduler).ToPropertyEx(this, x => x.ToDisplayImageFps);
        }

        private int _takingPicture;
        private async Task<Unit> TakePictureAsync(Unit unit, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _takingPicture, 1) == 1)
            {
                return Unit.Default;
            }


            SecondsUntilPictureIsTaken = 3;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 2;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 1;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 0;
            await Task.Delay(500, cancellationToken); // allow camera to adjust to the flash

            using Bitmap imageAsBitmap = ImageSource.Mat.ToBitmap();
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

            TakenImage = imageAsBitmap.ToBitmapSource();
            SecondsUntilPictureIsTaken = -1;
            PickupLine = _pickupLines[_random.Next(0, _pickupLines.Count - 1)];
            await Task.Delay(100, cancellationToken); // allow gui to update
            _outputImageRepository.Save(imageAsBitmap);
            await Task.Delay(3000, cancellationToken);
            TakenImage = null;
            PickupLine = null;

            Interlocked.Exchange(ref _takingPicture, 0);
            return Unit.Default;
        }


        private async Task<ImageSourceWithMat> LoadNextFrameAsync(bool forwards)
        {
            return await Task.Run(() =>
            {
                if (forwards)
                {
                    return _frameManager.Next(_capture.FrameSize);
                }

                return _frameManager.Previous(_capture.FrameSize);
            });
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
}