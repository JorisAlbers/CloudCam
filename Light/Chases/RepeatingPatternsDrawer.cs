using System.Collections;
using System.Collections.Generic;

namespace Light.Chases
{
    /// <summary>
    /// Repeats one or multiple pattern(s) over the length of the led strip.
    /// Each frame is a int[][] pattern repeated over the led strip.
    /// </summary>
    public class RepeatingPatternsDrawer : IEnumerator<int[]>
    {
        private int _lengthStrip;
        private int[][] _patterns;
        private int _index;
        private readonly int[] _buffer;

        /// <summary>
        /// Each array of int[]s in the patterns list will be repeated over the length of the ledstip.
        /// Each frame is the next pattern, repeated.
        /// </summary>
        public RepeatingPatternsDrawer(int lengthStrip, int[][] patterns)
        {
            _lengthStrip = lengthStrip;
            _patterns = patterns;
            _buffer = new int[lengthStrip];
        }

        
        public bool MoveNext()
        {
            int[] currentPattern = _patterns[_index];

            int patternsInStrip = _lengthStrip / currentPattern.Length;
            int leftPixelIndex = 0;

            for (int j = 0; j < patternsInStrip; j++)
            {
                leftPixelIndex = currentPattern.Length * j;
                for (int k = 0; k < currentPattern.Length; k++)
                {
                    int pixelIndex = leftPixelIndex + k;
                    _buffer[pixelIndex] = currentPattern[k];
                }
            }

            leftPixelIndex = leftPixelIndex + currentPattern.Length;
            // draw remaining pixels of the pattern that does not completely fit on the end of the led strip
            for (int j = 0; j < _lengthStrip % currentPattern.Length; j++)
            {
                int pixelIndex = leftPixelIndex + j;
                _buffer[pixelIndex] = currentPattern[j];
            }

            Current = _buffer;
            _index = ++_index % _patterns.Length;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public int[] Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            ;
        }
    }
}