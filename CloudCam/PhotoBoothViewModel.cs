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

namespace CloudCam
{
    public class PhotoBoothViewModel : ReactiveObject
    {
        private readonly Settings _settings;
        private readonly ImageRepository _frameRepository;
        private CancellationTokenSource _cancellationTokenSource;
        private int _frameWidth;
        private int _frameHeight;

        //TODO move to image cache class
        private int _currentFrameIndex;

        [ObservableAsProperty]
        public ImageSource ImageSource { get; }

        [ObservableAsProperty]
        public ImageSource Frame { get; }

        public ReactiveCommand<bool, ImageSource> NextFrame { get; }

        public PhotoBoothViewModel(Settings settings, CameraDevice device, ImageRepository frameRepository)
        {
            _settings = settings;
            _frameRepository = frameRepository;

            NextFrame = ReactiveCommand.CreateFromTask<bool,ImageSource>(LoadNextFrameAsync);
            NextFrame.ToPropertyEx(this, x => x.Frame, scheduler:RxApp.MainThreadScheduler);

            StreamVideo(_settings,device.OpenCdId).ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ImageSource);
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
                    Bitmap frameAsBitmap;


                    if (!videoCapture.Open(deviceId))
                    {
                        throw new ApplicationException($"Failed to open video device {deviceId}");
                    }

                    using var frame = new Mat();
                    while (true)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        videoCapture.Read(frame);

                        if (!frame.Empty())
                        {
                            if (_frameHeight == 0)
                            {
                                _frameHeight = frame.Height;
                                _frameWidth = frame.Width;
                            }

                            // Releases the lock on first not empty frame
                            frameAsBitmap = BitmapConverter.ToBitmap(frame);
                            BitmapSource lastFrameBitmapImage = frameAsBitmap.ToBitmapSource();
                            lastFrameBitmapImage.Freeze();
                            o.OnNext(lastFrameBitmapImage);

                            Thread.Sleep(33);
                        }
                    }
                });

                return new CompositeDisposable(scheduledWork, cts);
            });
        }
    }
}