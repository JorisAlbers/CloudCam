using System.Collections;
using System.Collections.Generic;

namespace Light.Chases
{
    /// <summary>
    /// Moves a pattern over the led strip from the start of the led strip till the end.
    /// </summary>
    public class MovingPatternDrawer : IEnumerator<int[]>
    {
        private readonly int[] _pattern;
        private readonly int _stripLength;
        private readonly IEnumerator<int[]> _internalEnumerator;
        private readonly int[] _buffer;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="stripLength">The length of the section to draw</param>
        /// <param name="pattern">The pattern to move</param>
        public MovingPatternDrawer(int stripLength, int[] pattern)
        {
            _stripLength = stripLength;
            _pattern = pattern;
            _buffer = new int[stripLength];
            _internalEnumerator = GetEnumerator();
        }

        public bool MoveNext()
        {
            return _internalEnumerator.MoveNext();
        }

        public void Reset()
        {
            _internalEnumerator.Reset();
        }

        public int[] Current => _internalEnumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
           _internalEnumerator.Dispose();
        }

        private IEnumerator<int[]> GetEnumerator()
        {
            while (true)
            {
                // Slide into view
                for (int i = 0; i < _pattern.Length - 1; i++)
                {
                    for (int j = 0; j < i + 1; j++)
                    {
                        int color = _pattern[_pattern.Length - 1 - i + j];
                        _buffer[j] = color;
                    }
                    yield return _buffer;
                }

                // Normal
                for (int i = 0; i < _stripLength - _pattern.Length + 1; i++)
                {
                    for (int j = 0; j < _pattern.Length; j++)
                    {
                        int color = _pattern[j];
                        _buffer[i + j] = color;
                    }

                    yield return _buffer;
                }

                // Slide out of view
                for (int i = 0; i < _pattern.Length - 1; i++)
                {
                    for (int j = 0; j < _pattern.Length - 1 - i; j++)
                    {
                        int color = _pattern[j];
                        _buffer[_stripLength - (_pattern.Length - 1 - j - i)] = color;
                    }

                    yield return _buffer;
                }
            }
        }
    }
}
