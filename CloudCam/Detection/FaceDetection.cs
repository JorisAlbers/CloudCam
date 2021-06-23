using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace CloudCam.Detection
{
    public class FaceDetection
    {
        private readonly Net _net;

        public FaceDetection(string caffeConfigFile, string caffeWeightFile)
        {
            _net = Net.ReadNetFromCaffe(caffeConfigFile, caffeWeightFile);
        }

        public Rect[] Detect(Mat image)
        {
            Mat inputBlob = CvDnn.BlobFromImage(image, 1.0, new Size(300, 300), new Scalar(104.0, 177.0, 123.0), false, false);
            _net.SetInput(inputBlob); // why use name?
            Mat detection = _net.Forward();

            Mat detectionMat = new Mat(detection.Size(2), detection.Size(3), MatType.CV_32F, detection.Data);

            List<Rect> rects = new List<Rect>();
            for (int i = 0; i < detectionMat.Rows; i++)
            {
                float confidence = detectionMat.At<float>(i, 2);
                if (confidence > 0.7)
                {
                    int x1 = (int) (detectionMat.At<float>(i, 3) * image.Cols);
                    int y1 = (int) (detectionMat.At<float>(i, 4) * image.Rows);
                    int x2 = (int) (detectionMat.At<float>(i, 5) * image.Cols);
                    int y2 = (int) (detectionMat.At<float>(i, 6) * image.Rows);
                    
                    // detect clipping
                    if (x1 < 0)
                        x1 = 0;
                    if (y1 < 0)
                        y1 = 0;
                    if (x2 > image.Cols)
                        x2 = image.Cols;
                    if (y2 > image.Rows)
                        y2 = image.Rows;

                    rects.Add(new Rect(x1, y1, x2- x1, y2 - y1));
                }
            }

            return rects.ToArray();
        }
    }
}
