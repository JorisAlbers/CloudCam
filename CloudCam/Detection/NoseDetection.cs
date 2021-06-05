using OpenCvSharp;

namespace CloudCam.Detection
{
    public class NoseDetection
    {
        private readonly CascadeClassifier _cascade;

        public NoseDetection(string cascadeFile)
        {
            _cascade = new CascadeClassifier(cascadeFile);
        }

        public Rect[] Detect(Mat image)
        {
            var grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGRA2GRAY);
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