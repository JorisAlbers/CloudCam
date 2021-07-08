using System;
using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class DebugDetection : IEffect
    {
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;
        private readonly EyesDetection _eyesDetection;

        public DebugDetection(FaceDetection faceDetection, NoseDetection noseDetection, EyesDetection eyesDetection)
        {
            _faceDetection = faceDetection;
            _noseDetection = noseDetection;
            _eyesDetection = eyesDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                mat.Rectangle(faceRect, Scalar.DarkRed);
                Mat roiColor = new Mat(mat, faceRect);
                DetectNose(roiColor, faceRect);
                DetectEyes(roiColor, faceRect);

            }
        }

        private void DetectEyes(Mat faceMat, Rect faceRect)
        {
            int desiredYAxis = (int)(faceRect.Height * 0.40);
            faceMat.Line(new Point(0, desiredYAxis), new Point(faceMat.Width, desiredYAxis), Scalar.Lavender);
            Rect[] eyes = _eyesDetection.Detect(faceMat);
            foreach (Rect eye in eyes)
            {
                faceMat.Rectangle(eye, Scalar.Purple);
            }

            if (eyes.Length > 1)
            {
                Rect surroundingRect = CalculateSurroundingRect(faceRect.Width,eyes[0], eyes[1]);
                faceMat.Rectangle(surroundingRect,Scalar.BurlyWood);
            }



        }

        private Rect CalculateSurroundingRect(int faceWidth, Rect eye1, Rect eye2)
        {
            int x1 = 0;
            int x2 = faceWidth;
            int y1 = Math.Min(eye1.Top, eye2.Top);
            int y2 = Math.Max(eye1.Bottom, eye2.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        private void DetectNose(Mat faceMat, Rect faceRect)
        {
            int desiredYAxis = (int)(faceRect.Height * 0.65);
            faceMat.Line(new Point(0, desiredYAxis), new Point(faceMat.Width, desiredYAxis), Scalar.Lavender);
            Rect[] noses = _noseDetection.Detect(faceMat);
            foreach (Rect nose in noses)
            {
                faceMat.Rectangle(nose, Scalar.SteelBlue);
            }
        }
    }
}