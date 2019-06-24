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
            else
            {
                //non null value
                for (int i = 0; i < _size; i++)
                {
                    if (ObjectEquals(_references[i], item))
                    {
                        return true;
                    }
                }
            }

            //ensure capacity
            if (_size>=_length)
            {
                _length += DefaultSize;
                Array.Resize(ref _references, _length);
            }

            _references[_size++]=item;
            return false;
        }

        private static bool ObjectEquals(object x, object y)
        {
            if (x != null)
            {
                return y != null && x == y;
            }
            return y == null;
        }
    }
}
