using OpenCvSharp;
using OpenCvSharp.XPhoto;

namespace CloudCam.Effect
{
    public class OilPainting : IEffect
    {
        public void Apply(Mat mat)
        {
           CvXPhoto.OilPainting(mat,mat,6,1);
        }
    }
}