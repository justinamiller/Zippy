using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Utility
{
    sealed class FastLookup<TKey, TValue> : IEnumerable
    {
        TKey[] _key = Array.Empty<TKey>();
        TValue[] _value = Array.Empty<TValue>();
        int _index = 0;

        public int Length
        {
            get
            {
                return _index;
            }
        }

        public IReadOnlyCollection<TKey> Keys
        {
            get
            {
                return Array.AsReadOnly(_key);
            }
        }

        public IReadOnlyCollection<TValue> Values
        {
            get
            {
                return Array.AsReadOnly(_value);
            }
        }

        public FastLookup()
        {
        }

        public void Clear()
        {
            _key = Array.Empty<TKey>();
            _value = Array.Empty<TValue>();
            _index = 0;
        }

        public void Add(TKey key, TValue value)
        {
            Array.Resize(ref _key, _index + 1);
            Array.Resize(ref _value, _index + 1);

            _key[_index] = key;
            _value[_index] = value;
            _index++;
        }

        public bool GetValue(TKey key, out TValue value)
        {
            for (var i = 0; i < _index; i++)
            {
                if (RuntimeHelpers.Equals(_key[i], key))
                {
                    value = _value[i];
                    return true;
                }
            }

            value = default;
            return false;
        }

       public  IEnumerator GetEnumerator()
        {
            return new FastLookupEnumerator(_key, _value);
        }

        //private enumerator class
        private class FastLookupEnumerator : IEnumerator
        {
            readonly TKey[] _key;
            readonly TValue[] _value;
            int _position = -1;

            //constructor
            public FastLookupEnumerator(TKey[] key, TValue[] value)
            {
                _key = key;
                _value = value;
            }

            public IEnumerator GetEnumerator()
            {
                return this;
            }


            //IEnumerator
            public bool MoveNext()
            {
                _position++;
                return (_position < _key.Length);
            }

            //IEnumerator
            public void Reset()
            {
                _position = -1;
            }

            //IEnumerator
            public object Current
            {
                get
                {
                    try
                    {
                        return new DictionaryEntry(_key[_position], _value[_position]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }  //end nested class
    }
}
