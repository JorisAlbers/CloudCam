using System;
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

            var candidates = _cascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 2,
                flags: HaarDetectionTypes.DoRoughSearch | HaarDetectionTypes.ScaleImage,
                minSize: new Size(30, 30)
            );

            if (candidates.Length < 2)
            {
                return candidates;
            }

            int desiredYAxis = (int)(image.Height * 0.65);
            Rect bestRect = candidates[0];
            int bestDistance = CalculateDistance(candidates[0].Top, candidates[0].Bottom, desiredYAxis);
            
            for (int i = 1; i < candidates.Length; i++)
            {
                int distance = CalculateDistance(candidates[i].Top, candidates[i].Bottom, desiredYAxis);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestRect = candidates[i];
                }
            }

            return new []{bestRect};
        }

        private int CalculateDistance(int x1, int x2, int desired)
        {
            if (x1 < desired && x2 > desired)
            {
                return 0;
            }

            int d1 = Math.Abs(x1 - desired);
            int d2 = Math.Abs(x2 - desired);
            return Math.Min(d1, d2);
        }
        
    }
}