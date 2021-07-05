using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Hats : IEffect
    {
        private readonly Mat _hat;
        private readonly ImageSettings _settings;
        private readonly Mat _hatMask;
        private readonly Mat _hatMaskInverse;
        private readonly FaceDetection _faceDetection;

        public Hats(Mat hat, ImageSettings settings, FaceDetection faceDetection)
        {
            _hatMask = hat.ExtractChannel(3); // extract Alpha
            _hatMaskInverse = new Mat();
            Cv2.BitwiseNot(_hatMask, _hatMaskInverse);
            Cv2.CvtColor(hat, hat, ColorConversionCodes.BGRA2BGR);
            _hat = hat;
            _settings = settings;

            _faceDetection = faceDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                int hatWidth = (int)(faceRect.Width * 1.10);

                int hatHeight = hatWidth * _hat.Height / _hat.Width;

                int adjustment = (hatWidth - faceRect.Width) / 2;
                // Center the hat on top of the face
                int x1 = faceRect.Left - adjustment;
                int x2 = faceRect.Right + adjustment;
                int y1 = faceRect.Top - hatHeight;
                int y2 = faceRect.Top;

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

                Mat hatNew = new Mat();
                Cv2.Resize(_hat, hatNew, new Size(hatWidth, hatHeight),
                    interpolation: InterpolationFlags.Area);

                Mat mask = new Mat();
                Cv2.Resize(_hatMask, mask, new Size(hatWidth, hatHeight),
                    interpolation: InterpolationFlags.Area);
                Mat maskInverse = new Mat();
                Cv2.Resize(_hatMaskInverse, maskInverse, new Size(hatWidth, hatHeight),
                    interpolation: InterpolationFlags.Area);


                Rect rectangle = new Rect(x1, y1, x2 - x1, y2 - y1);

                Mat roi = new Mat(mat, rectangle);
                Mat roiBg = new Mat();
                Cv2.BitwiseAnd(roi, roi, roiBg, maskInverse);

                Mat roiFg = new Mat();
                Cv2.BitwiseAnd(hatNew, hatNew, roiFg, mask);
                Mat d = new Mat();
                Cv2.Add(roiBg, roiFg, d);

                d.CopyTo(mat.SubMat(rectangle));
            }
        }
    }
}