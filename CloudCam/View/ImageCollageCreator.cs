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

        public async Task<Bitmap> Create(Bitmap[] foregrounds, String pickupLine, CancellationToken cancellationToken)
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

                // Overlay pickup line on the bottom of the image
                float fontSize = 72;
                FontStyle fontStyle = FontStyle.Bold;
                using Font font = new Font("Arial", fontSize, fontStyle);
                using SolidBrush brush = new SolidBrush(Color.Blue);

                RectangleF textRectangle = new RectangleF(80, copy.Height - 460, copy.Width - 160, 430);
                SizeF textSize = gr.MeasureString(pickupLine, font);
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                // If the text is too big, make it smaller
                if (gr.MeasureString(pickupLine, font).Width > textRectangle.Width*3)
                {
                    fontSize = ((textRectangle.Width * 3) / gr.MeasureString(pickupLine, font).Width) * fontSize;
                    using Font smallerFont = new Font("Arial", fontSize, fontStyle);
                    gr.DrawString(pickupLine, smallerFont, brush, textRectangle, stringFormat);
                }
                else
                {
                    gr.DrawString(pickupLine, font, brush, textRectangle, stringFormat);
                }

                /*gr.DrawString(pickupLine, font, brush, new RectangleF(40, 1600-460, 600-80, 390), new StringFormat(ContentAlignment.MiddleCenter));*/
            }, cancellationToken);

            return copy;
        }
    }
}