using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CloudCam.View;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

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
            await Task.Run(async () =>
            {
                try
                {
                    long lastErrorAt = Environment.TickCount;
                    Mat previousMat = null;
                    Mat currentMat = null;
                    int startTicks = Environment.TickCount;
                    int frames = 0;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            currentMat = _matBuffer.GetNextForDisplay(previousMat);
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
                        }
                        catch (Exception ex)
                        {
                            if (lastErrorAt < Environment.TickCount - 1000)
                            {
                                Log.Logger.Error(ex, "Failed to transform image to display image!");
                                lastErrorAt = Environment.TickCount;
                            }
                        }
                        finally
                        {
                            previousMat = currentMat;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex,"Failed to run image to display converter!");
                }
               
            }, token);
        }
    }
}