using OpenCvSharp;

namespace CloudCam
{
    public interface IEffect
    {
        void Apply(Mat mat);
    }
}