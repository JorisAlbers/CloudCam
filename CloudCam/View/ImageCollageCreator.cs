using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Serilog;
using Color = System.Drawing.Color;

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
                System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Bold;
                using (Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudCam.Resources.Fonts.tradizional_DEMO.otf"))
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
                            using (SolidBrush brush = new SolidBrush(Color.Blue))
                            {
                                RectangleF textRectangle = new RectangleF(80, copy.Height - 460, copy.Width - 160, 430);
                                StringFormat stringFormat = new StringFormat();
                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Center;

                                // If the text is too big, make it smaller
                                if (gr.MeasureString(pickupLine, font).Width > textRectangle.Width * 3)
                                {
                                    fontSize = ((textRectangle.Width * 3) / gr.MeasureString(pickupLine, font).Width) * fontSize;
                                    font = new Font(fontFamily, fontSize, fontStyle);
                                }
                                gr.DrawString(pickupLine, font, brush, textRectangle, stringFormat);
                            }
                        }

                    }
                }
                /*PrivateFontCollection fontCollection = new PrivateFontCollection();
                string fontFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Fonts\tradizional_DEMO.otf");
                fontCollection.AddFontFile(fontFilePath);

                // Register the custom font
                *//*IntPtr fontPtr = Marshal.AllocCoTaskMem(fontCollection.GetTotalMemory());
                fontCollection.SaveMemory(fontPtr);*//*

                // Create the font object
                using System.Drawing.FontFamily fontFamily = new System.Drawing.FontFamily(fontCollection.Families[0].Name, fontCollection);

                float fontSize = 72;
                FontStyle fontStyle = FontStyle.Bold;
                Font font = new Font(fontFamily, fontSize, fontStyle);
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
                    font = new Font(fontFamily , fontSize, fontStyle);
                }
                gr.DrawString(pickupLine, font, brush, textRectangle, stringFormat);*/
            }, cancellationToken);

            return copy;
        }
    }
}