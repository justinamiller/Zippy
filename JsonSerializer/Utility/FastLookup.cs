using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Utility
{
    sealed class FastLookup<TKey, TValue> : IEnumerable
    {
        TKey[] _types = Array.Empty<TKey>();
        TValue[] _codes = Array.Empty<TValue>();
        int _index = 0;

        public int Length
        {
            get
            {
                return _index;
            }
        }

        public FastLookup()
        {
        }

        public void Clear()
        {
            _types = Array.Empty<TKey>();
            _codes = Array.Empty<TValue>();
            _index = 0;
        }

        public void Add(TKey type, TValue code)
        {
            Array.Resize(ref _types, _index + 1);
            Array.Resize(ref _codes, _index + 1);

            _types[_index] = type;
            _codes[_index] = code;
            _index++;
        }

        public bool GetValue(TKey key, out TValue value)
        {
            for (var i = 0; i < _index; i++)
            {
                if (RuntimeHelpers.Equals(_types[i], key))
                {
                    value = _codes[i];
                    return true;
                }
            }

            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}
