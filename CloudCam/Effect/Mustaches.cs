using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Mustaches : IEffect
    {
        private readonly ImageOverlayer _overlayer;
        private readonly Size _mustacheSize;
        private readonly ImageSettings _settings;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;

        public Mustaches(ImageOverlayer overlayer,Size mustacheSize, ImageSettings settings, FaceDetection faceDetection, NoseDetection noseDetection)
        {
            _overlayer = overlayer;
            _mustacheSize = mustacheSize;
            _settings = settings;

            _faceDetection = faceDetection;
            _noseDetection = noseDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                Mat roiColor = new Mat(mat, faceRect);

                Rect[] noses = _noseDetection.Detect(roiColor);
                // only do for a single nose
                if (noses.Length == 0)
                {
                    continue;
                }

                Rect noseRect = noses[0];

                int mustacheWidth = 3 * noseRect.Width;
                int mustacheHeight = mustacheWidth * _mustacheSize.Height / _mustacheSize.Width;


                // Center the hat on the bottom of the nose
                int x1 = noseRect.X - (mustacheWidth / 4);
                int x2 = noseRect.X + noseRect.Width + (mustacheWidth / 4);
                int y1 = noseRect.Y + noseRect.Height - (mustacheHeight / 2);
                int y2 = noseRect.Y + noseRect.Height + (mustacheHeight / 2);

                // Check for clipping
                if (x1 < 0)
                    x1 = 0;
                if (y1 < 0)
                    y1 = 0;
                if (x2 > faceRect.Width)
                    x2 = faceRect.Width;
                if (y2 > faceRect.Height)
                    y2 = faceRect.Height;

                // Re-calculate the width and height of the hat image
                mustacheWidth = x2 - x1;
                mustacheHeight = y2 - y1;

                Size mustacheSize = new Size(mustacheWidth, mustacheHeight);
                Rect rectangle = new Rect(faceRect.X + x1, faceRect.Y + y1, x2 - x1, y2 - y1);
                _overlayer.Overlay(mat, mustacheSize, rectangle);
            }
        }
    }
}