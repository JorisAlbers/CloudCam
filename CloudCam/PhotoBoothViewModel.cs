using System;
using System.Drawing;
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
        private readonly CameraDevice _device;
        private CancellationTokenSource _cancellationTokenSource;
        private int _frameWidth;
        private int _frameHeight;

        [Reactive] public ImageSource VideoFrame { get; set; }

        public PhotoBoothViewModel(Settings settings, CameraDevice device)
        {
            _settings = settings;
            _device = device;
        }

        public async Task Start()
        {
            _cancellationTokenSource?.Cancel();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                var videoCapture = new VideoCapture();
                Bitmap frameAsBitmap;
                

                if (!videoCapture.Open(_device.OpenCvId))
                {
                    throw new ApplicationException($"Failed to open video device {_device.Name}");
                }

                using var frame = new Mat();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    int ticks = Environment.TickCount;
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
                        VideoFrame = lastFrameBitmapImage;
                    }

                    // 30 FPS
                    await Task.Delay(33);
                }
            });
        }
    }
}