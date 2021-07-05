using OpenCvSharp;

namespace CloudCam
{
    public class EffectImageWithSettings : EffectImage
    {
        public ImageSettings Settings { get; }

        public EffectImageWithSettings(Mat image, ImageSettings settings) : base(image)
        {
            Settings = settings;
        }
    }
}