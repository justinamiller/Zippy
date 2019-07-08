using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Zippy.Utility;

namespace Zippy.Serialize
{
    sealed class ReferenceCheck
    {
        private const int DefaultSize = 10;
        private object[] _references;
        private int _size;
        private int _length;

        public ReferenceCheck() : this(DefaultSize) { }

        public ReferenceCheck(int capacity)
        {
            if (0 >= capacity)
            {
                capacity = DefaultSize;
            }

            _length = capacity;
            _references = new object[capacity];
            _size = 0;
        }

        public bool IsReferenced(object item)
        {
            if (item == null)
            {
                //nulls are never references.
                return false;
            }

            if (Exists(item))
            {
                return true;
            }

            //new object need to add it.
            AddReference(item);

            return false;
        }

        private void AddReference(object item)
        {
            //ensure capacity
            if (_size >= _length)
            {
                //need to grow.
                var len = _length;
                _length += DefaultSize;
                var temp = new object[_length];
                Array.Copy(_references, 0, temp, 0, len);
                _references = temp;
            }

            _references[_size++] = item;
        }

        private bool Exists(object item)
        {
            //non null value
            for (int i = 0; i < _size; i++)
            {
                if (_references[i] == item)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
