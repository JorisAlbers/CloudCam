using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class DebugDetection : IEffect
    {
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;

        public DebugDetection(FaceDetection faceDetection, NoseDetection noseDetection)
        {
            _faceDetection = faceDetection;
            _noseDetection = noseDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                int desiredYAxis = (int)(faceRect.Height * 0.65);

                mat.Rectangle(faceRect, Scalar.DarkRed);
                Mat roiColor = new Mat(mat, faceRect);
                roiColor.Line(new Point(0,desiredYAxis), new Point(roiColor.Width,desiredYAxis), Scalar.Lavender);

                Rect[] noses = _noseDetection.Detect(roiColor);
                foreach (Rect nose in noses)
                {
                    roiColor.Rectangle(nose, Scalar.SteelBlue);
                }
            }
        }
    }
}