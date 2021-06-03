using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light
{
    public class NeoPixelUsbBridge : IEnumerator<int[]>
    {
        private int _blockCount;
        private int _blockLenght;
        private int _position;
        private int _color;
        private int[] pixels;

        public NeoPixelUsbBridge(int LedCount, int Blocks, int BlockLength)
        {
            pixels = new int[LedCount];
            _blockCount = Blocks;
            _blockLenght = BlockLength;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            //Clear all the pixels
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 0;

            for (int b = 0; b < _blockCount; b++)
            {
                var blockIndex = ((pixels.Length / _blockCount) * b) + _position;
                for (int i = 0; i < _blockLenght; i++)
                {
                    var pixIndex = (blockIndex + i) % pixels.Length;
                    var colorIndex = (_color + i) % LedEffects.sinColorTable.Length;
                    pixels[pixIndex] = LedEffects.sinColorTable[colorIndex];
                }
            }

            _position = (_position + 1) % pixels.Length;
            _color = (_color + 1) % LedEffects.sinColorTable.Length;
            Current = pixels;
            return true;
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public int[] Current { private set; get; }

        object IEnumerator.Current => Current;
    }
}