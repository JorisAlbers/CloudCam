using OpenCvSharp;

namespace CloudCam.Effect
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