using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Hats : IEffect
    {
        private readonly ImageOverlayer _imageOverlayer;
        private readonly Size _hatSize;
        private readonly ImageSettings _settings;
        private readonly FaceDetection _faceDetection;

        public Hats(ImageOverlayer imageOverlayer, Size hatSize, ImageSettings settings, FaceDetection faceDetection)
        {
            _imageOverlayer = imageOverlayer;
            _hatSize = hatSize;
            _settings = settings;
            _faceDetection = faceDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                int hatWidth = (int)(faceRect.Width * _settings.WidthRatio);

                int hatHeight = hatWidth * _hatSize.Height / _hatSize.Width;
                
                int adjustment = (hatWidth - faceRect.Width) / 2;
                // Center the hat on top of the face

                int yAdjustment = (hatHeight / 100 )* _settings.Y;
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

                _imageOverlayer.Overlay(mat, new Size(hatWidth, hatHeight), new Rect(x1, y1, x2 - x1, y2 - y1));
            }
        }
    }
}