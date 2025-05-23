﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

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

        public Mat GetNextForCapture(Mat previous)
        {
            if (previous != null)
            {
                if (previous.Empty())
                {
                    // An error occurred during capturing. return the same mat for capturing.
                    return previous;
                }
                
                Mat editing = Interlocked.Exchange(ref _readyForEditing, previous);
                if (editing != null)
                {
                    // was not used for editing.
                    _readyForCapture.Add(editing);
                }
            }

            if (_readyForCapture.TryTake(out Mat mat))
            {
               return mat;
            }

            // TODO run error instead of return previous, we are out of buffers!
            return previous;
        }

        public Mat GetNextForEditing(Mat previous)
        {
            if (previous != null)
            {
                Mat display = Interlocked.Exchange(ref _readyForDisplay, previous);
                if (display != null && display != previous)
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
