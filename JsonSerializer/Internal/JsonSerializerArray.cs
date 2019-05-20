using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer.Internal
{
    sealed class JsonSerializerArray : List<object>
    {
        public JsonSerializerArray()
        {
        }

        public JsonSerializerArray(int capacity) : base(capacity)
        {
        }

        public override string ToString()
        {
            return Serializer.SerializeObject(this) ?? string.Empty;
        }
    }
}
