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
        private bool _areOn;
        private int[] pixels;

        public Flicker(int LedCount)
        {
            pixels = new int[LedCount];
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool MoveNext()
        {
            //Clear all the pixels
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = _areOn ? LedEffects.RgbToInt(0, 0, 0) : LedEffects.RgbToInt(255, 255, 255);

            _areOn = !_areOn;
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
