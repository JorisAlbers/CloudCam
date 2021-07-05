using OpenCvSharp;

namespace CloudCam
{
    public class EffectImage
    {
        public Mat Image { get; }

        public EffectImage(Mat image)
        {
            Image = image;
        }
    }
}