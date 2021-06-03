using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace CloudCam
{
    public class WebcamCapture
    {
        private readonly int _camId;
        private readonly MatBuffer _matBuffer;

        public Size FrameSize { get; private set; }

        public WebcamCapture(int camId, MatBuffer matBuffer)
        {
            _camId = camId;
            _matBuffer = matBuffer;
        }

        public async Task CaptureAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var videoCapture = new VideoCapture();
                if (!videoCapture.Open(_camId))
                {
                    throw new ApplicationException($"Failed to open video device {_camId}");
                }

                FrameSize = SetMaxResolution(videoCapture);

                // Get first frame for dimensions
                Mat frame = _matBuffer.GetNextForCapture(null);
                videoCapture.Read(frame);
                while (!cancellationToken.IsCancellationRequested)
                {
                    frame = _matBuffer.GetNextForCapture(frame);
                    videoCapture.Read(frame);
                }

                videoCapture.Dispose();

            }, cancellationToken);
        }
        
        private Size SetMaxResolution(VideoCapture videoCapture)
        {
            Size[] commonResolutions = new Size[]
            {
                new Size(160, 120),
                new Size(176, 144),
                new Size(320, 240),
                new Size(352, 288),
                new Size(640, 360),
                new Size(640, 480),
                new Size(800, 600),
                new Size(960, 720),
                new Size(1280, 720),
                //new Size(1920, 1080),
                //new Size(2560, 1472),
            };

            for (int i = commonResolutions.Length - 1; i >= 0; i--)
            {
                // First set,
                videoCapture.Set(VideoCaptureProperties.FrameWidth, commonResolutions[i].Width);
                videoCapture.Set(VideoCaptureProperties.FrameHeight, commonResolutions[i].Height);

                // Then check if available
                Size actual = new Size(
                    videoCapture.Get(VideoCaptureProperties.FrameWidth),
                    videoCapture.Get(VideoCaptureProperties.FrameHeight));

                if (actual.Equals(commonResolutions[i]))
                {
                    return actual;
                }
            }

            // None available, return the current one
            return new Size(
                videoCapture.Get(VideoCaptureProperties.FrameWidth),
                videoCapture.Get(VideoCaptureProperties.FrameHeight));
        }
    }
}
