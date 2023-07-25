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
    public class ForegroundLocator: ReactiveObject
    {
        private readonly MatBuffer _matBuffer;
        [Reactive] public float Fps { get; private set; }

        public ForegroundLocator(MatBuffer matBuffer)
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
                    Mat currentMat = null;
                    int startTicks = Environment.TickCount;
                    int frames = 0;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            currentMat = _matBuffer.GetNextForEditing(previousMat);
                            IFaceDetectionEffect faceDetectionEffect = settings.Effect;

                            if (currentMat != null)
                            {
                                if (faceDetectionEffect == null)
                                {
                                    settings.CurrentForegrounds = null;
                                }
                                else
                                {
                                    settings.CurrentForegrounds = faceDetectionEffect.Find(currentMat);
                                }

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
                                Log.Logger.Error(ex, "Failed to apply effect!");
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
                    Log.Logger.Error(ex, "Failed to run image transform!");
                }
               
            }, token);
        }
    }
}