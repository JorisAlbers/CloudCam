﻿
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Light
{
    public class LedController : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _numberOfPixelsSide;
        private readonly int _numberOfPixelsFront;
        private string _usbPort;
        private int _framesPerSecond;
        private int BYTESPERPIXEL = 8;
        private int _baudRate = 2400000;
        static readonly byte[] _bitTriplet = new byte[]
        {
            0x5b, 0x1b, 0x53, 0x13,
            0x5a, 0x1a, 0x52, 0x12
        };
        private byte[] _uartBuffer;
        private IEnumerator<int[]> _sideChase;
        private IEnumerator<int[]> _frontChase;

        public LedController(int numberOfPixelsFront, int numberOfPixelsSide, string usbPort, int framesPerSecond)
        {
            _numberOfPixelsSide = numberOfPixelsSide;
            _numberOfPixelsFront = numberOfPixelsFront;
            _usbPort = usbPort;
            _framesPerSecond = framesPerSecond;

            int totalPixels = numberOfPixelsSide + numberOfPixelsFront;
            _uartBuffer = new byte[totalPixels * BYTESPERPIXEL];   
        }

        public async Task StartAsync()
        {
            Log.Logger.Information("Starting led controller");
            await Task.Run(() =>
            {
                using (var serialPort = new SerialPort(_usbPort, _baudRate, Parity.None, 7, StopBits.One))
                {
                    if (!serialPort.IsOpen)
                        serialPort.Open();

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        var frontChase = _frontChase;
                        if (frontChase != null)
                        {
                            frontChase.MoveNext();
                            TranslateColors(frontChase.Current, _uartBuffer, 0);
                        }

                        var sideChase = _sideChase;
                        if (sideChase != null)
                        {
                            sideChase.MoveNext();
                            TranslateColors(sideChase.Current, _uartBuffer, _numberOfPixelsFront);
                        }

                        serialPort.BaseStream.Write(_uartBuffer, 0, _uartBuffer.Length);
                        serialPort.BaseStream.Flush();
                        Thread.Sleep(1000 / _framesPerSecond);
                    }
                }
            });

            // Cleanup here
        }

        public void StartAnimationAtFront(IEnumerator<int[]> chase)
        {
            Log.Logger.Information("Starting led animation at front");
            _frontChase = chase;
        }

        public void StartAnimationAtSide(IEnumerator<int[]> chase)
        {
            Log.Logger.Information("Starting led animation at side");
            _sideChase = chase;
        }

        private void TranslateColors(int[] colors, byte[] UartData, int startPosition)
        {
            // Fill in the side pixels
            for (int i = 0; i < colors.Length; i++)
            {
                var color = ~colors[i];
                var pixOffset = (startPosition + i ) * BYTESPERPIXEL;

                //only 8 permutations so no need to use a for loop
                UartData[pixOffset] = _bitTriplet[(color >> 21) & 0x07];
                UartData[pixOffset + 1] = _bitTriplet[(color >> 18) & 0x07];
                UartData[pixOffset + 2] = _bitTriplet[(color >> 15) & 0x07];
                UartData[pixOffset + 3] = _bitTriplet[(color >> 12) & 0x07];
                UartData[pixOffset + 4] = _bitTriplet[(color >> 9) & 0x07];
                UartData[pixOffset + 5] = _bitTriplet[(color >> 6) & 0x07];
                UartData[pixOffset + 6] = _bitTriplet[(color >> 3) & 0x07];
                UartData[pixOffset + 7] = _bitTriplet[color & 0x07];
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
