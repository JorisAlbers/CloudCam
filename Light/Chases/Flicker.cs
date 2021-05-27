using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light
{
    public class Flicker : IEnumerator<int[]>
    {
        private int _blockCount;
        private int _blockLenght;
        private int _position;
        private int _color;
        private int[] pixels;

        public Flicker(int LedCount, int Blocks, int BlockLength)
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
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public int[] Current { private set; get; }

        object IEnumerator.Current => Current;
    }
}
