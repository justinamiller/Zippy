﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Utility
{
    sealed class FastLookup<TKey, TValue> : IEnumerable
    {
        TKey[] _key = ArrayExtensions.EmptyArray<TKey>();
        TValue[] _value = ArrayExtensions.EmptyArray<TValue>();
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
                return new System.Collections.ObjectModel.ReadOnlyCollection<TKey>(_key);
            }
        }

        public IReadOnlyCollection<TValue> Values
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<TValue>(_value);
            }
        }

        public FastLookup()
        {
        }

        public void Clear()
        {
            _key = ArrayExtensions.EmptyArray<TKey>();
            _value = ArrayExtensions.EmptyArray<TValue>();
            _index = 0;
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
            {
                //do not add already exists.
                return;
            }

            int newSize = _index + 1;
            Array.Resize(ref _key, newSize);
            Array.Resize(ref _value, newSize);

            _key[_index] = key;
            _value[_index] = value;
            _index++;
        }

        public bool ContainsKey(TKey key)
        {
            for (var i = 0; i < _index; i++)
            {
                if (RuntimeHelpers.Equals(_key[i], key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
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
