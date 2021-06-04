using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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

        public async Task StartAsync(TransformationSettings settings, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                Mat previousMat = null;

                int startTicks = Environment.TickCount;
                int frames = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    IEffect effect = settings.Effect;
                    Mat currentMat = _matBuffer.GetNextForEditing(previousMat);
                    if (currentMat != null)
                    {
                        if (effect != null)
                        {
                            effect.Apply(currentMat);
                        }

                        previousMat = currentMat;

                        if (++frames > 50)
                        {
                            int elapsedMilliseconds = Environment.TickCount - startTicks;
                            Fps = 50.0f / (elapsedMilliseconds / 1000.0f);
                            frames = 0;
                            startTicks = Environment.TickCount;
                        }
                    }
                }
            }, cancellationToken);
        }
    }
}