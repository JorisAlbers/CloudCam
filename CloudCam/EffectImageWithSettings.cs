using OpenCvSharp;

namespace CloudCam
{
    public class EffectImageWithSettings : EffectImage
    {
        private readonly ImageSettings _settings;

        public EffectImageWithSettings(Mat image, ImageSettings settings) : base(image)
        {
            _settings = settings;
        }
    }
}