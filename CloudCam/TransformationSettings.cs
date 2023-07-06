using System.Collections.Generic;
using CloudCam.Effect;
using CloudCam.View;
using OpenCvSharp;

namespace CloudCam
{
    public class TransformationSettings
    {
        public IFaceDetectionEffect Effect { get; set; }

        public List<ForegroundImage> CurrentForegrounds { get; set; }
    }

}