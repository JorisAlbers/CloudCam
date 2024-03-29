﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace CloudCam
{
    public class WebcamCapture : ReactiveObject
    {
        private readonly CameraDevice _cameraDevice;
        private readonly MatBuffer _matBuffer;
        private VideoCapture _videoCapture;

        public Size FrameSize { get; private set; }

        [Reactive] public float Fps { get; private set; }

        public WebcamCapture(CameraDevice cameraDevice, MatBuffer matBuffer)
        {
            _cameraDevice = cameraDevice;
            _matBuffer = matBuffer;
        }

        public async Task Initialize()
        {
            var x = await Task.Run(async () =>
            {
                // Get the latest openCV ID of the webcam. It could be changed if its index changed)
                while(!_cameraDevice.TryGetOpenCvId(out int openCvId))
                {
                    Log.Logger.Warning("Failed to connect to webcam, trying again in a few seconds");
                    Thread.Sleep(5000);
                }
                
                var capture  = new VideoCapture(_cameraDevice.OpenCdId, VideoCaptureAPIs.DSHOW);

                if (!capture.Open(_cameraDevice.OpenCdId, VideoCaptureAPIs.DSHOW))
                {
                    throw new ApplicationException($"Failed to open video device {_cameraDevice.OpenCdId}");
                }

                var frameSize =  SetMaxResolution(capture);

                return (capture, frameSize);

            });

            _videoCapture = x.capture;
            FrameSize = x.frameSize;
        }


        public async Task CaptureAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Get first frame for dimensions
                    Mat frame = _matBuffer.GetNextForCapture(null);
                    _videoCapture.Read(frame);
                    long lastErrorAt = Environment.TickCount;

                    int startTicks = Environment.TickCount;
                    int frames = 0;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            frame = _matBuffer.GetNextForCapture(frame);

                            if (!_videoCapture.Read(frame))
                            {
                                throw new WebcamFailedException();
                            }

                            Cv2.Flip(frame, frame, FlipMode.Y);

                            if (++frames > 50)
                            {
                                int elapsedMilliseconds = Environment.TickCount - startTicks;
                                Fps = 50.0f / (elapsedMilliseconds / 1000.0f);
                                frames = 0;
                                startTicks = Environment.TickCount;
                            }
                        }
                        catch (WebcamFailedException e)
                        {
                            Log.Logger.Warning("Webcam failed. Attempting to reconnect");
                            _videoCapture.Dispose();
                            await Initialize();
                        }
                        catch (Exception ex)
                        {
                            if (lastErrorAt < Environment.TickCount - 1000)
                            {
                                Log.Logger.Error(ex, "Failed to capture frame from webcam!");
                                lastErrorAt = Environment.TickCount;
                            }
                        }
                    }

                    _videoCapture.Dispose();

                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Failed to run webcam capturewebcam!");
                }

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

    public class WebcamFailedException : Exception
    {
    }
}
