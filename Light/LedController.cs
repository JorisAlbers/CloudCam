using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Light
{
    public class LedController : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _numberOfPixels;
        private string _usbPort;
        private int _framesPerSecond;
        private int BYTESPERPIXEL = 8;
        private int _baudRate = 3000000;
        static readonly byte[] _bitTriplet = new byte[]
        {
            0x5b, 0x1b, 0x53, 0x13,
            0x5a, 0x1a, 0x52, 0x12
        };
        private int[] _colorBuffer;
        private byte[] _uartBuffer;
        private IEnumerator<int[]> _chase;


        public LedController(int numberOfPixels, string usbPort, int framesPerSecond)
        {
            _numberOfPixels = numberOfPixels;
            _usbPort = usbPort;
            _framesPerSecond = framesPerSecond;

            _colorBuffer = new int[numberOfPixels];
            _uartBuffer = new byte[numberOfPixels * BYTESPERPIXEL];   
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                using (var serialPort = new SerialPort(_usbPort, _baudRate, Parity.None, 7, StopBits.One))
                {
                    if (!serialPort.IsOpen)
                        serialPort.Open();

                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        var chase = _chase;
                        chase.MoveNext();
                        TranslateColors(chase.Current, _uartBuffer);

                        serialPort.BaseStream.Write(_uartBuffer, 0, _uartBuffer.Length);
                        serialPort.BaseStream.Flush();
                        Thread.Sleep(1000 / _framesPerSecond);
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.X)
                            break;
                    }
                }
            });

            // Cleanup here
        }

        public void StartAnimation(IEnumerator<int[]> chase)
        {
            _chase = chase;
        }

        private void TranslateColors(int[] colors, byte[] UartData)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var pixOffset = i * BYTESPERPIXEL;

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

        public void Flash()
        {
            
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
