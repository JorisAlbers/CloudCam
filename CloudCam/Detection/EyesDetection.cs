using System;
using OpenCvSharp;

namespace CloudCam.Detection
{
    public class EyesDetection
    {
        private readonly CascadeClassifier _cascade;

        public EyesDetection(string cascadeFile)
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

            return candidates;

            // We should find two eyes

            if (candidates.Length < 3)
            {
                return candidates;
            }

            int desiredYAxis = (int)(image.Height * 0.40);
            Rect bestRect1 = candidates[0] , bestRect2 = candidates[1];
            int bestDistance1 = CalculateDistance(candidates[0].Top, candidates[0].Bottom, desiredYAxis);
            int bestDistance2 = CalculateDistance(candidates[1].Top, candidates[1].Bottom, desiredYAxis);
            
            for (int i = 1; i < candidates.Length; i++)
            {
                int distance = CalculateDistance(candidates[i].Top, candidates[i].Bottom, desiredYAxis);
                if (distance < bestDistance1)
                {
                    bestDistance1 = distance;
                    bestRect1 = candidates[i];
                    continue;
                }

                if(distance < bestDistance2)
                {
                    bestDistance2 = distance;
                    bestRect2 = candidates[i];
                }
            }

            return new[] { bestRect1 , bestRect2};
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