using System;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Color = System.Windows.Media.Color;
using Size = OpenCvSharp.Size;

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly ImageRepository _frameRepository;
        private readonly OutputImageRepository _outputImageRepository;
        private CancellationTokenSource _cancellationTokenSource;

        //TODO move to image cache class
        private int _currentFrameIndex;
        private readonly WebcamCapture _capture;
        private readonly MatBuffer _matBuffer;
        private readonly ImageTransformer _imageTransformer;

        [Reactive] public int SecondsUntilPictureIsTaken { get; set; } = -1;

        [ObservableAsProperty]
        public ImageSourceWithMat ImageSource { get; }

        [ObservableAsProperty]
        public ImageSourceWithMat Frame { get; }

        public ReactiveCommand<bool, ImageSourceWithMat> NextFrame { get; }

        public ReactiveCommand<Unit,Unit> TakePicture { get; }

        public PhotoBoothViewModel(Settings settings, CameraDevice device, ImageRepository frameRepository,
            OutputImageRepository outputImageRepository)
        {
            _settings = settings;
            _frameRepository = frameRepository;
            _outputImageRepository = outputImageRepository;
            _matBuffer = new MatBuffer();

            NextFrame = ReactiveCommand.CreateFromTask<bool, ImageSourceWithMat>(LoadNextFrameAsync);
            NextFrame.ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Unit>(TakePictureAsync);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            _capture = new WebcamCapture(device.OpenCdId, _matBuffer);
            _ = _capture.CaptureAsync(cancellationTokenSource.Token);

            TransformationSettings transformationSettings = new TransformationSettings();
            _imageTransformer = new ImageTransformer(_matBuffer);
            Task t = _imageTransformer.StartAsync(transformationSettings, cancellationTokenSource.Token);

            
            Mat previousMat = null;
            Observable.Interval(TimeSpan.FromMilliseconds(33)) // cap at 30 fps
                .Select(_ => _matBuffer.GetNextForDisplay(previousMat))
                .Where(x => previousMat != x)
                .Do((x) => previousMat = x)
                .Select((x) => new ImageSourceWithMat(x.ToBitmapSource(), x.ToBitmap())) // TODO only convert to bitmap if photo is taken
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x=>x.ImageSource);
        }

        private async Task<Unit> TakePictureAsync(Unit unit, CancellationToken cancellationToken)
        {
            SecondsUntilPictureIsTaken = 3;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 2;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 1;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 0;
            await Task.Delay(100, cancellationToken); // allow GUI to update

            Bitmap frameAsBitmap = Frame.Bitmap;
            Bitmap imageAsBitmap = ImageSource.Bitmap;
            
            await Task.Run(() =>
            {
                // Overlay frame on top of image
                using (Graphics gr = Graphics.FromImage(imageAsBitmap))
                {
                    gr.DrawImage(frameAsBitmap, new System.Drawing.Point(0, 0));
                }
                _outputImageRepository.Save(imageAsBitmap);
            }, cancellationToken);
            SecondsUntilPictureIsTaken = -1;
            return Unit.Default;
        }


        private async Task<ImageSourceWithMat> LoadNextFrameAsync(bool forwards)
        {
            return await Task.Run(() =>
            {
                Mat image;
                if (forwards)
                {
                    _currentFrameIndex++;
                    if (_currentFrameIndex > _frameRepository.Count -1)
                    {
                        _currentFrameIndex = 0;
                    }

                    image = _frameRepository[_currentFrameIndex];
                }
                else
                {
                    _currentFrameIndex--;
                    if (_currentFrameIndex == -1)
                    {
                        _currentFrameIndex = _frameRepository.Count - 1;
                    }

                    image = _frameRepository[_currentFrameIndex];
                }

                Cv2.Resize(image, image, _capture.FrameSize);
                var imageSource = image.ToBitmapSource();
                var bitmap = image.ToBitmap();
                imageSource.Freeze();
                return  new ImageSourceWithMat(imageSource, bitmap);
            });
        }
    }

    public class ImageSourceWithMat
    {
        public ImageSource ImageSource { get; }
        public Bitmap Bitmap { get; }

        public ImageSourceWithMat(ImageSource imageSource, Bitmap bitmap)
        {
            ImageSource = imageSource;
            Bitmap = bitmap;
        }
    }
}