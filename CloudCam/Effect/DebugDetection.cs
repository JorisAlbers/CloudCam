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