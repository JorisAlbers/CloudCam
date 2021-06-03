using System.Threading;
using System.Threading.Tasks;

namespace CloudCam
{
    public class ImageTransformer
    {
        private readonly MatBuffer _matBuffer;

        public ImageTransformer(MatBuffer matBuffer)
        {
            _matBuffer = matBuffer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    
                }
            }, cancellationToken);
        }
    }
}