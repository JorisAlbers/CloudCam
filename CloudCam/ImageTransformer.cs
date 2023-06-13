using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using CloudCam.Effect;
using OpenCvSharp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam
{
    public class ImageTransformer: ReactiveObject
    {
        private readonly MatBuffer _matBuffer;
        [Reactive] public float Fps { get; private set; }

        public ImageTransformer(MatBuffer matBuffer)
        {
            _matBuffer = matBuffer;
        }

        public async Task StartAsync(TransformationSettings settings, CancellationToken token)
        {
            long lastErrorAt = Environment.TickCount;
            await Task.Run(async () =>
            {
                try
                {
                    Mat previousMat = null;

                    int startTicks = Environment.TickCount;
                    int frames = 0;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            IEffect effect = settings.Effect;
                            Mat currentMat = _matBuffer.GetNextForEditing(previousMat);
                            if (currentMat != null)
                            {
                                if (effect != null)
                                {
                                    effect.Apply(currentMat);
                                }

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
                        catch (Exception ex)
                        {
                            if (lastErrorAt < Environment.TickCount - 1000)
                            {
                                Log.Logger.Error(ex, "Failed to apply effect!");
                                lastErrorAt = Environment.TickCount;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Failed to run image transform!");
                }
               
            }, token);
        }
    }
}