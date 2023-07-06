using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using CloudCam.Detection;
using CloudCam.View;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Glasses : IFaceDetectionEffect
    {
        private readonly Size _glassesSize;
        private readonly EffectImageWithSettings _settings;
        private readonly FaceDetection _faceDetection;
        private readonly EyesDetection _eyesDetection;

        public Glasses(Size glassesSize, EffectImageWithSettings settings, FaceDetection faceDetection, EyesDetection eyesDetection)
        {
            _glassesSize = glassesSize;
            _settings = settings;

            _faceDetection = faceDetection;
            _eyesDetection = eyesDetection;
        }

        public List<ForegroundImage> Find(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            var rects = new List<Rect>();
            foreach (Rect faceRect in faces)
            {
                Mat roiColor = new Mat(mat, faceRect);

                Rect[] eyes = _eyesDetection.Detect(roiColor);
                
                if (eyes.Length < 2)
                {
                    continue;
                }

                Rect surroundingRect = CalculateSurroundingRect(faceRect.Width,eyes[0], eyes[1]);
                
                int x1 = surroundingRect.X;
                int x2 = surroundingRect.X + surroundingRect.Width;
                int y1 = surroundingRect.Y ;
                int y2 = surroundingRect.Y + surroundingRect.Height;

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
                int glassesWidth = x2 - x1;
                int glassesHeight = y2 - y1;

                // TODO apply image settings here

                Size glassesSize = new Size(glassesWidth, glassesHeight);
                Rect rectangle = new Rect(faceRect.X + x1, faceRect.Y + y1, x2 - x1, y2 - y1);
                rects.Add(rectangle);
            }

            return rects.Select(x => new ForegroundImage(_settings.Image, x)).ToList();
        }

        private Rect CalculateSurroundingRect(int faceWidth,Rect eye1, Rect eye2)
        {
            int x1 = 0;
            int x2 = faceWidth;
            int y1 = Math.Min(eye1.Top, eye2.Top);
            int y2 = Math.Max(eye1.Bottom, eye2.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
    }
}