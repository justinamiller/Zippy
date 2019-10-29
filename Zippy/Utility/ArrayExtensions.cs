using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zippy.Utility
{
   static class ArrayExtensions
    {
        // Useful in number of places that return an empty byte array to avoid unnecessary memory allocation.
        private static class EmptyArrayHelper<T>
        {
            public static readonly T[] Value = new T[0];
        }

        /// <summary>
        /// creation of 0 allocation Array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] EmptyArray<T>()
        {
            return EmptyArrayHelper<T>.Value;
        }
    }
}
