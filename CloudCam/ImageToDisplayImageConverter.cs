using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CloudCam
{
    public class ImageToDisplayImageConverter : ReactiveObject
    {
        private readonly MatBuffer _matBuffer;

        public ImageSourceWithMat ImageSourceWithMat { get; private set; }

        [Reactive] public float Fps { get; private set; }

        public ImageToDisplayImageConverter(MatBuffer matBuffer)
        {
            _matBuffer = matBuffer;
        }

        public async Task StartAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                Mat previousMat = null;
                int startTicks = Environment.TickCount;
                int frames = 0;
                while (!token.IsCancellationRequested)
                {
                    Mat currentMat = _matBuffer.GetNextForDisplay(previousMat);
                    if (currentMat != null)
                    {
                        BitmapSource imageSource = currentMat.ToBitmapSource();
                        imageSource.Freeze();
                        ImageSourceWithMat = new ImageSourceWithMat(imageSource, currentMat);

                        if (++frames > 50)
                        {
                            int elapsedMilliseconds = Environment.TickCount - startTicks;
                            Fps = 50.0f / (elapsedMilliseconds / 1000.0f);
                            frames = 0;
                            startTicks = Environment.TickCount;
                        }
                    }
                    previousMat = currentMat;
                }
            }, token);
        }
    }
}