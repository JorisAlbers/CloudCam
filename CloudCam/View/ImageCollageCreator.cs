using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CloudCam.View
{
    public class ImageCollageCreator
    {
        private readonly Bitmap _backgroundImage;
        private readonly Rectangle[] _overlayAreas;
        // set line size to 3.5
        private readonly float LINE_SIZE = (float)3.5;

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
                using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudCam.Resources.Fonts.daniel.ttf"))
                {
                    if (fontStream != null)
                    {
                        byte[] fontData = new byte[fontStream.Length];
                        fontStream.Read(fontData, 0, (int)fontStream.Length);

                        PrivateFontCollection fontCollection = new PrivateFontCollection();
                        unsafe
                        {
                            fixed (byte* fontDataPtr = fontData)
                            {
                                fontCollection.AddMemoryFont((IntPtr)fontDataPtr, fontData.Length);
                            }
                        }

                        // Create the font object
                        Font font;
                        using (System.Drawing.FontFamily fontFamily = fontCollection.Families[0])
                        {
                            font = new Font(fontFamily, fontSize);

                            // Use the font in your drawing code
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(251,246,222)))
                            {
                                RectangleF textRectangle = new RectangleF(80, copy.Height - 460, copy.Width - 160, 430);
                                StringFormat stringFormat = new StringFormat();
                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Center;

                                // If the text is too big, make it smaller
                                if (gr.MeasureString(pickupLine, font).Width > textRectangle.Width * LINE_SIZE)
                                {
                                    fontSize = ((textRectangle.Width * LINE_SIZE) / gr.MeasureString(pickupLine, font).Width) * fontSize;
                                    font = new Font(fontFamily, fontSize, fontStyle);
                                }
                                gr.DrawString(pickupLine, font, brush, textRectangle, stringFormat);
                            }
                        }

                    }
                }
            }, cancellationToken);

            return copy;
        }
    }
}