using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace JsonSerializer.Internal
{
    class ReferenceEqualsEqualityComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            // put objects in a bucket based on their reference
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
