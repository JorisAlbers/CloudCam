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
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Size = OpenCvSharp.Size;

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly ImageRepository _frameRepository;
        private readonly OutputImageRepository _outputImageRepository;
        private CancellationTokenSource _cancellationTokenSource;

        private Size _frameSize;

        //TODO move to image cache class
        private int _currentFrameIndex;

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

            NextFrame = ReactiveCommand.CreateFromTask<bool, ImageSourceWithMat>(LoadNextFrameAsync);
            NextFrame.ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

            TakePicture = ReactiveCommand.CreateFromTask<Unit, Unit>(TakePictureAsync);

            StreamVideo(device.OpenCdId).ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ImageSource);
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
            await Task.Run(() =>
            {
                _outputImageRepository.Save(ImageSource.Mat);
            });
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

                Cv2.Resize(image, image, _frameSize);
                var imageSource = image.ToBitmapSource();
                imageSource.Freeze();
                return  new ImageSourceWithMat(imageSource, image);
            });
        }

        private IObservable<ImageSourceWithMat> StreamVideo(int deviceId)
        {
            IScheduler scheduler = DefaultScheduler.Instance;
            return Observable.Create<ImageSourceWithMat>(o =>
            {
                var cts = new CancellationTokenSource();
                var scheduledWork = scheduler.Schedule(() =>
                {
                    var videoCapture = new VideoCapture();
                    if (!videoCapture.Open(deviceId))
                    {
                        throw new ApplicationException($"Failed to open video device {deviceId}");
                    }

                    _frameSize = SetMaxResolution(videoCapture);
                    
                    // Get first frame for dimensions
                    using var frame = new Mat();
                    videoCapture.Read(frame);
                    while (!cts.Token.IsCancellationRequested)
                    {
                        videoCapture.Read(frame);

                        if (!frame.Empty())
                        {
                            var imageSource = frame.ToBitmapSource();
                            imageSource.Freeze();
                            o.OnNext(new ImageSourceWithMat(imageSource, frame));

                            Thread.Sleep(33);
                        }
                    }
                    o.OnCompleted();
                });

                return new CompositeDisposable(scheduledWork, cts);
            });
        }

        private Size SetMaxResolution(VideoCapture videoCapture)
        {
            Size[] commonResolutions = new Size[]
            {
                new Size(160, 120),
                new Size(176, 144),
                new Size(320, 240),
                new Size(352, 288),
                new Size(640, 360),
                new Size(640, 480),
                new Size(800, 600),
                new Size(960, 720),
                new Size(1280, 720),
                //new Size(1920, 1080),
                //new Size(2560, 1472),
            };

            for (int i = commonResolutions.Length - 1; i >= 0; i--)
            {
                // First set,
                videoCapture.Set(VideoCaptureProperties.FrameWidth, commonResolutions[i].Width);
                videoCapture.Set(VideoCaptureProperties.FrameHeight, commonResolutions[i].Height);
                
                // Then check if available
                Size actual = new Size(
                    videoCapture.Get(VideoCaptureProperties.FrameWidth),
                    videoCapture.Get(VideoCaptureProperties.FrameHeight));

                if (actual.Equals(commonResolutions[i]))
                {
                    return actual;
                }
            }

            // None available, return the current one
            return new Size(
                videoCapture.Get(VideoCaptureProperties.FrameWidth),
                videoCapture.Get(VideoCaptureProperties.FrameHeight));
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