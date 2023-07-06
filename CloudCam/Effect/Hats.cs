using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using CloudCam.Detection;
using CloudCam.View;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Hats : IFaceDetectionEffect
    {
        private readonly Size _hatSize;
        private readonly EffectImageWithSettings _settings;
        private readonly FaceDetection _faceDetection;

        public Hats(Size hatSize, EffectImageWithSettings settings, FaceDetection faceDetection)
        {
            _hatSize = hatSize;
            _settings = settings;
            _faceDetection = faceDetection;
        }

        public List<ForegroundImage> Find(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            var rects = new List<Rect>();
            foreach (Rect faceRect in faces)
            {
                int hatWidth = (int)(faceRect.Width * _settings.Settings.WidthRatio);

                int hatHeight = hatWidth * _hatSize.Height / _hatSize.Width;
                
                int adjustment = (hatWidth - faceRect.Width) / 2;
                // Center the hat on top of the face

                int yAdjustment = (hatHeight / 100 )* _settings.Settings.Y;
                int y2 = faceRect.Top + yAdjustment;
                int y1 = y2 - hatHeight;

                int x1 = faceRect.Left - adjustment;
                int x2 = faceRect.Right + adjustment;
     
                // Check for clipping // TODO cut image instead of resize
                if (x1 < 0)
                    x1 = 0;
                if (y1 < 0)
                    y1 = 0;
                if (x2 > mat.Width)
                    x2 = mat.Width;
                if (y2 > mat.Height)
                    y2 = mat.Height;


                // Re-calculate the width and height of the hat image
                hatWidth = x2 - x1;
                hatHeight = y2 - y1;

                if (hatWidth == 0 || hatHeight == 0)
                {
                    continue;
                }
                // TODO apply image settings here.
                rects.Add(new Rect(x1, y1, x2 - x1, y2 - y1));
            }

            return  rects.Select(x => new ForegroundImage(_settings.Image, x)).ToList();
        }
    }
}