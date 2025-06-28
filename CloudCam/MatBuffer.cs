using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using Serilog;

namespace CloudCam
{
    public class MatBuffer
    {
        private readonly ConcurrentBag<Mat> _readyForCapture;
        private Mat _readyForEditing;
        private Mat _readyForDisplay;


        public MatBuffer()
        {
            _readyForCapture = new ConcurrentBag<Mat>(new Mat[] {new Mat(), new Mat(), new Mat(), new Mat()});
        }

        public Mat GetNextForCapture(Mat matWithCapture)
        {
            if (matWithCapture != null)
            {
                if (matWithCapture.Empty())
                {
                    // An error occurred during capturing. return the same mat for capturing.
                    Log.Logger.Error("MatBuffer:GetNextForCapture an error occured during capturing. in GetNextForCapture!");
                    return matWithCapture;
                }
                
                Mat editing = Interlocked.Exchange(ref _readyForEditing, matWithCapture);
                if (editing != null)
                {
                    Log.Logger.Warning($"Editing did not happen, frame returned for capture. frame id = ${editing.GetHashCode()}");
                    // was not used for editing.
                    _readyForCapture.Add(editing);
                }
            }

            if (_readyForCapture.TryTake(out Mat mat))
            {
               return mat;
            }

            Log.Logger.Error("MatBuffer:GetNextForCapture ran out of buffers in GetNextForCapture!");
            // TODO run error instead of return previous, we are out of buffers!
            return matWithCapture;
        }

        public Mat GetNextForEditing(Mat matWithEffect)
        {
            if (matWithEffect != null)
            {
                Mat display = Interlocked.Exchange(ref _readyForDisplay, matWithEffect);
                if (display != null && display != matWithEffect)
                {
                    // was not used for display.
                    _readyForCapture.Add(display);
                }
            }

            Mat mat = Interlocked.Exchange(ref _readyForEditing, null);
            return mat;
        }

        public Mat GetNextForDisplay(Mat previous)
        {
            if (previous != null)
            {
                _readyForCapture.Add(previous);
            }

            return Interlocked.Exchange(ref _readyForDisplay, null);
        }

    }
}
