using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Light
{
    public class LedController
    {
        private int _numberOfPixels;
        private string _usbPort;
        const int BYTESPERPIXEL = 8;
        static int baudRate = 3000000;
        static readonly byte[] bitTriplets = new byte[]
        {
            0x5b, 0x1b, 0x53, 0x13,
            0x5a, 0x1a, 0x52, 0x12
        };
        private int[] _colorBuffer;
        private byte[] _uartBuffer;

        public LedController(int numberOfPixels, string usbPort)
        {
            _numberOfPixels = numberOfPixels;
            _usbPort = usbPort;

            _colorBuffer = new int[numberOfPixels];
            _uartBuffer = new byte[numberOfPixels * BYTESPERPIXEL];

            
        }

        public void Start()
        {
            using (var serialPort = new SerialPort(_usbPort, baudRate, Parity.None, 7, StopBits.One))
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();

                //var chase = new LedEfects.ColorChase(pixelCount, 3, 6);

                for (; ; )
                {
                    //chase.MoveNext();
                    //TranslateColors(chase.Current, _uartBuffer);

                    serialPort.BaseStream.Write(_uartBuffer, 0, _uartBuffer.Length);
                    serialPort.BaseStream.Flush();
                    Thread.Sleep(1);
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.X)
                        break;
                }
            }
        }

        public void TranslateColors(int[] colors, byte[] UartData)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var pixOffset = i * BYTESPERPIXEL;

                //only 8 permutations so no need to use a for loop
                UartData[pixOffset] = bitTriplets[(color >> 21) & 0x07];
                UartData[pixOffset + 1] = bitTriplets[(color >> 18) & 0x07];
                UartData[pixOffset + 2] = bitTriplets[(color >> 15) & 0x07];
                UartData[pixOffset + 3] = bitTriplets[(color >> 12) & 0x07];
                UartData[pixOffset + 4] = bitTriplets[(color >> 9) & 0x07];
                UartData[pixOffset + 5] = bitTriplets[(color >> 6) & 0x07];
                UartData[pixOffset + 6] = bitTriplets[(color >> 3) & 0x07];
                UartData[pixOffset + 7] = bitTriplets[color & 0x07];
            }
        }

        public void Flash()
        {
            
        }
    }
}
