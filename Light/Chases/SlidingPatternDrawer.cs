using System.Collections;
using System.Collections.Generic;

namespace Light.Chases
{
    /// <summary>
    /// Repeats a pattern over the ledstrip and moves the start point of the pattern each
    /// frame by +1.
    /// </summary>
    public class SlidingPatternDrawer : IEnumerator<int[]>
    {
        private readonly int[] _pattern;
        private readonly int _stripLength;
        private int _index;
        private readonly int[] _buffer;

        public SlidingPatternDrawer(int stripLength, int[] pattern)
        {
            _stripLength = stripLength;
            _pattern = pattern;
            _buffer = new int[stripLength];
        }

        public bool MoveNext()
        {
            for (int j = 0; j < _stripLength; j++)
            {
                int color = _pattern[(j + _index) % (_pattern.Length)];
                _buffer[j] = color;
            }

            _index = ++_index % _pattern.Length;
            Current = _buffer;
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