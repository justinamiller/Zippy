using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zippy.Serialize
{
    sealed class ReferenceCheck
    {
        private const int DefaultSize = 10;
        private object[] _references = new object[DefaultSize];
        private int _size = 0;
        private int _length = DefaultSize;
        bool _hasNull = false;

        public ReferenceCheck()
        {
        }

        public bool IsReferenced(object item)
        {
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
                _length += DefaultSize;
                var temp = new object[_length];
                Array.Copy(_references, 0, temp, 0, _references.Length);
                _references = temp;
            }

            _references[_size++] = item;
        }

        private bool Exists(object item)
        {
            if (item == null)
            {
                //handling for null value.
                if (_hasNull)
                {
                    return true;
                }
                _hasNull = true;
                return false;
            }

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
