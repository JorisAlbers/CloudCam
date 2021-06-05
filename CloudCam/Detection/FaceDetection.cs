using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CloudCam.Detection
{
    public class FaceDetection
    {
        private readonly CascadeClassifier _cascade;

        public FaceDetection(string faceCascadeFile)
        {
            _cascade = new CascadeClassifier(faceCascadeFile);
        }

        public Rect[] Detect(Mat image)
        {
            var grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGRA2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);

            return _cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 2,
                flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                minSize: new Size(30, 30)
            );
        }
    }
}
