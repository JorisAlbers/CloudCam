using System.Collections.Generic;
using System.Windows.Documents;
using CloudCam.View;
using OpenCvSharp;

namespace CloudCam.Effect
{
    public interface IEffect
    {
        void Apply(Mat mat);
    }

    public interface IFaceDetectionEffect
    {
        List<ForegroundImage> Find(Mat frame);
    }
}   