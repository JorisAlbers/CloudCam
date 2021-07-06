using OpenCvSharp;

namespace CloudCam.Effect
{
    public class ImageOverlayer
    {
        private Mat _foregroundMask;
        private Mat _foregroundMaskInverse;
        private Mat _foreground;


        public ImageOverlayer(Mat foreground)
        {
            _foreground = foreground;
            _foregroundMask = foreground.ExtractChannel(3); // extract Alpha
            Cv2.EqualizeHist(_foregroundMask, _foregroundMask);
            _foregroundMaskInverse = new Mat();
            Cv2.BitwiseNot(_foregroundMask, _foregroundMaskInverse);
            Cv2.CvtColor(foreground, foreground, ColorConversionCodes.BGRA2BGR);
        }

        public void Overlay(Mat background, Size foregroundSize, Rect foregroundRectangle)
        {
            Mat foregroundNew = new Mat();
            Cv2.Resize(_foreground, foregroundNew, foregroundSize,
                interpolation: InterpolationFlags.Area);

            Mat mask = new Mat();
            Cv2.Resize(_foregroundMask, mask, foregroundSize,
                interpolation: InterpolationFlags.Area);
            Mat maskInverse = new Mat();
            Cv2.Resize(_foregroundMaskInverse, maskInverse, foregroundSize,
                interpolation: InterpolationFlags.Area);

            Mat roi = new Mat(background, foregroundRectangle);
            Mat roiBg = new Mat();
            Cv2.BitwiseAnd(roi, roi, roiBg, maskInverse);

            Mat roiFg = new Mat();
            Cv2.BitwiseAnd(foregroundNew, foregroundNew, roiFg, mask);
            Mat d = new Mat();
            Cv2.Add(roiBg, roiFg, d);

            d.CopyTo(background.SubMat(foregroundRectangle));
        }
    }
}