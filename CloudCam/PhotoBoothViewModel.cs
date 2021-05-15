using System;
using System.Drawing;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Size = OpenCvSharp.Size;

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly ImageRepository _frameRepository;
        private CancellationTokenSource _cancellationTokenSource;

        private Size _frameSize;

        //TODO move to image cache class
        private int _currentFrameIndex;

        [Reactive] public int SecondsUntilPictureIsTaken { get; set; } = -1;

        [ObservableAsProperty]
        public ImageSource ImageSource { get; }

        [ObservableAsProperty]
        public ImageSource Frame { get; }

        public ReactiveCommand<bool, ImageSource> NextFrame { get; }

        public ReactiveCommand<Unit,Mat> TakePicture { get; }

        public PhotoBoothViewModel(Settings settings, CameraDevice device, ImageRepository frameRepository)
        {
            _settings = settings;
            _frameRepository = frameRepository;

            NextFrame = ReactiveCommand.CreateFromTask<bool,ImageSource>(LoadNextFrameAsync);
            NextFrame.ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Mat>(TakePictureAsync);

            StreamVideo(_settings,device.OpenCdId).ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ImageSource);
        }

        private async Task<Mat> TakePictureAsync(Unit unit, CancellationToken cancellationToken)
        {
            SecondsUntilPictureIsTaken = 3;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 2;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 1;
            await Task.Delay(1000, cancellationToken);
            SecondsUntilPictureIsTaken = 0;
            await Task.Delay(1000); // take picture
            SecondsUntilPictureIsTaken = -1;
            return new Mat();
        }


        private async Task<ImageSource> LoadNextFrameAsync(bool forwards)
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

                Cv2.Resize(image, image, _frameSize);
                var image2 = image.ToBitmap();
                var lastFrameBitmapImage = image2.ToBitmapSourceWithAlpha();
                lastFrameBitmapImage.Freeze();
                return lastFrameBitmapImage;
            });
        }

        private IObservable<ImageSource> StreamVideo(Settings settings, int deviceId)
        {
            IScheduler scheduler = DefaultScheduler.Instance;
            return Observable.Create<ImageSource>(o =>
            {
                var cts = new CancellationTokenSource();
                var scheduledWork = scheduler.Schedule(() =>
                {
                    var videoCapture = new VideoCapture();
                    if (!videoCapture.Open(deviceId))
                    {
                        throw new ApplicationException($"Failed to open video device {deviceId}");
                    }

                    // Get first frame for dimensions
                    using var frame = new Mat();
                    videoCapture.Read(frame);
                    _frameSize = new Size(frame.Width, frame.Height);
                    
                    while (!cts.Token.IsCancellationRequested)
                    {
                        videoCapture.Read(frame);

                        if (!frame.Empty())
                        {
                            var frameAsBitmap = frame.ToBitmap();
                            BitmapSource lastFrameBitmapImage = frameAsBitmap.ToBitmapSource();
                            lastFrameBitmapImage.Freeze();
                            o.OnNext(lastFrameBitmapImage);

                            Thread.Sleep(33);
                        }
                    }
                    o.OnCompleted();
                });

                return new CompositeDisposable(scheduledWork, cts);
            });
        }
    }
}