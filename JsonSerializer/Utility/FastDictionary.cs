using System;
using System.Collections.Generic;
using System.Text;

namespace Zippy.Utility
{
    public sealed class FastDictionary
    {
        private Type[] _types = Array.Empty<Type>();
        private TypeCode[] _codes = Array.Empty<TypeCode>();
        int _index = 0;

        public void Add(Type type, TypeCode code)
        {
            Array.Resize(ref _types, _index + 1);
            Array.Resize(ref _codes, _index + 1);

            _types[_index] = type;
            _codes[_index] = code;
            _index++;
        }

        public TypeCode Get(Type type)
        {
            for (var i = 0; i < _index; i++)
            {
                if (_types[i] == type)
                {
                    return _codes[i];
                }
            }

            return TypeCode.Empty;
        }
    }
}
