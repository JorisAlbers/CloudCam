using CloudCam.Detection;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public class Mustaches : IEffect
    {
        private readonly Mat _mustache;
        private readonly Mat _mustacheMask;
        private readonly Mat _mustacheMaskInverse;
        private readonly FaceDetection _faceDetection;
        private readonly NoseDetection _noseDetection;

        public Mustaches(Mat mustache, FaceDetection faceDetection, NoseDetection noseDetection)
        {
            _mustacheMask = mustache.ExtractChannel(3); // extract Alpha
            _mustacheMaskInverse = new Mat();
            Cv2.BitwiseNot(_mustacheMask, _mustacheMaskInverse);
            Cv2.CvtColor(mustache, mustache, ColorConversionCodes.BGRA2BGR);
            _mustache = mustache;

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
                int mustacheHeight = mustacheWidth * _mustache.Height / _mustache.Width;


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

                Mat mustacheNew = new Mat();
                Cv2.Resize(_mustache, mustacheNew, new Size(mustacheWidth, mustacheHeight),
                    interpolation: InterpolationFlags.Area);

                Mat mask = new Mat();
                Cv2.Resize(_mustacheMask, mask, new Size(mustacheWidth, mustacheHeight),
                    interpolation: InterpolationFlags.Area);
                Mat maskInverse = new Mat();
                Cv2.Resize(_mustacheMaskInverse, maskInverse, new Size(mustacheWidth, mustacheHeight),
                    interpolation: InterpolationFlags.Area);


                Rect rectangle = new Rect(x1, y1, x2 - x1, y2 - y1);

                Mat roi = new Mat(roiColor, rectangle);
                Mat roiBg = new Mat();
                Cv2.BitwiseAnd(roi, roi, roiBg, maskInverse);

                Mat roiFg = new Mat();
                Cv2.BitwiseAnd(mustacheNew, mustacheNew, roiFg, mask);
                Mat d = new Mat();
                Cv2.Add(roiBg, roiFg, d);

                d.CopyTo(roiColor.SubMat(rectangle));
            }
        }
    }

    public class Hats : IEffect
    {
        private readonly Mat _hat;
        private readonly Mat _hatMask;
        private readonly Mat _hatMaskInverse;
        private readonly FaceDetection _faceDetection;

        public Hats(Mat hat, FaceDetection faceDetection)
        {
            _hatMask = hat.ExtractChannel(3); // extract Alpha
            _hatMaskInverse = new Mat();
            Cv2.BitwiseNot(_hatMask, _hatMaskInverse);
            Cv2.CvtColor(hat, hat, ColorConversionCodes.BGRA2BGR);
            _hat = hat;

            _faceDetection = faceDetection;
        }

        public void Apply(Mat mat)
        {
            Rect[] faces = _faceDetection.Detect(mat);
            foreach (Rect faceRect in faces)
            {
                int hatWidth = faceRect.Width;
                int hatHeight = hatWidth * _hat.Height / _hat.Width;


                // Center the hat on top of the face
                int x1 = faceRect.Left;
                int x2 = faceRect.Right;
                int y1 = faceRect.Top - hatHeight;
                int y2 = faceRect.Top;

                // Check for clipping
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