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
                mat.Rectangle(faceRect, Scalar.DarkRed);
                Mat roiColor = new Mat(mat, faceRect);

                Rect[] noses = _noseDetection.Detect(roiColor);
                foreach (Rect nose in noses)
                {
                    mat.Rectangle(nose, Scalar.SteelBlue);
                }
            }
        }
    }
}