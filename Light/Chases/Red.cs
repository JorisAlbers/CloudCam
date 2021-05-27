using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Light
{
    public class Red : IEnumerator<int[]>
    {
        private int[] pixels;

        public Red(int LedCount)
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
                pixels[i] = LedEffects.RgbToInt(255, 0, 0);

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
