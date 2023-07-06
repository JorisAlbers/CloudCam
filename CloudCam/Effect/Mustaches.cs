using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using CloudCam.Detection;
using CloudCam.View;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Mustaches : IFaceDetectionEffect
    {
        private readonly Size _mustacheSize;
        private readonly EffectImageWithSettings _settings;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;

        public Mustaches(Size mustacheSize, EffectImageWithSettings settings, FaceDetection faceDetection, NoseDetection noseDetection)
        {
            _mustacheSize = mustacheSize;
            _settings = settings;

            _faceDetection = faceDetection;
            _noseDetection = noseDetection;
        }

        public List<ForegroundImage> Find(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            
            var rects = new List<Rect>();

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

                // TODO apply image settings here
                Size mustacheSize = new Size(mustacheWidth, mustacheHeight);
                Rect rectangle = new Rect(faceRect.X + x1, faceRect.Y + y1, x2 - x1, y2 - y1);
                rects.Add(rectangle);
            }

            return rects.Select(x => new ForegroundImage(_settings.Image, x)).ToList();
        }
    }
}