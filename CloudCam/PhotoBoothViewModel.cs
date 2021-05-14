using System;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
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
        private CancellationTokenSource _cancellationTokenSource;
        private int _frameWidth;
        private int _frameHeight;

        [ObservableAsProperty]
        public ImageSource ImageSource { get; }
    
        public PhotoBoothViewModel(Settings settings, CameraDevice device)
        {
            _settings = settings;
            StreamVideo(_settings,device.OpenCdId).ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ImageSource);
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