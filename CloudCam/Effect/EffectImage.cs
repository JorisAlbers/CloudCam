using OpenCvSharp;

namespace CloudCam.Effect
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