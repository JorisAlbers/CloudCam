using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light
{
    public class SingleColor : IEnumerator<int[]>
    {
        private readonly int _color;
        private int[] pixels;

        public SingleColor(int LedCount, int color)
        {
            _color = color;
            pixels = new int[LedCount];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            Current = pixels;

        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
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
