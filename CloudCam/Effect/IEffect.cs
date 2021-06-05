using OpenCvSharp;

namespace CloudCam.Effect
{
    public interface IEffect
    {
        void Apply(Mat mat);
    }
}