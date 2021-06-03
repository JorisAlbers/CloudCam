using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CloudCam
{
    public class ImageTransformer
    {
        private readonly MatBuffer _matBuffer;

        public ImageTransformer(MatBuffer matBuffer)
        {
            _matBuffer = matBuffer;
        }

        public async Task StartAsync(TransformationSettings settings, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                Mat previousMat = null;

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
                    }
                }
            }, cancellationToken);
        }
    }
}