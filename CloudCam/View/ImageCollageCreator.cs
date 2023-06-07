using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CloudCam.View
{
    public class ImageCollageCreator
    {
        private readonly Bitmap _backgroundImage;
        private readonly Rectangle[] _overlayAreas;

        public ImageCollageCreator(Bitmap backgroundImage, Rectangle[] overlayAreas)
        {
            _backgroundImage = backgroundImage;
            _overlayAreas = overlayAreas;
        }

        public async Task<Bitmap> Create(Bitmap[] foregrounds, CancellationToken cancellationToken)
        {
            Log.Logger.Information("Creating image collage");
            if (foregrounds.Length != _overlayAreas.Length)
            {
                throw new ArgumentException(
                    $"Length of {nameof(foregrounds)} ({foregrounds.Length}) is not equal to length of {nameof(_overlayAreas)} ({_overlayAreas.Length})!");
            }

            Bitmap copy = new Bitmap(_backgroundImage);
            await Task.Run(() =>
            {
                // Overlay frame on top of image
                using Graphics gr = Graphics.FromImage(copy);
                for (int i = 0; i < foregrounds.Length; i++)
                {
                    gr.DrawImage(foregrounds[i], _overlayAreas[i]);
                }
            }, cancellationToken);

            return copy;

        }
    }
}