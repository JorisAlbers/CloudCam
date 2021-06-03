using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace CloudCam
{
    public class ImageToDisplayImageConverter
    {
        private readonly MatBuffer _matBuffer;

        public ImageSourceWithMat ImageSourceWithMat { get; private set; }

        public ImageToDisplayImageConverter(MatBuffer matBuffer)
        {
            _matBuffer = matBuffer;
        }

        public async Task StartAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                Mat previousMat = null;
                while (!token.IsCancellationRequested)
                {
                    Mat currentMat = _matBuffer.GetNextForDisplay(previousMat);
                    if (currentMat != null)
                    {
                        BitmapSource imageSource = currentMat.ToBitmapSource();
                        imageSource.Freeze();
                        ImageSourceWithMat = new ImageSourceWithMat(imageSource, currentMat);
                    }
                    previousMat = currentMat;
                }
            }, token);
        }
    }
}