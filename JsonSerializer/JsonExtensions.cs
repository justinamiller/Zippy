using System;
using System.Diagnostics.CodeAnalysis;
using Zippy.Utility;

namespace Zippy
{
    public static class JsonExtensions
    {
        /// <summary>
        /// will serialize an object into json string
        /// </summary>
        /// <param name="instance">object</param>
        /// <returns></returns>
        [SuppressMessage("brain-overload", "S1541")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging should not affect program behavior.")]
        public static string ToJson(this object instance)
        { 
            return JSON.SerializeObjectToString(instance);
        }

        public static string BeautifyJson(string input)
        {
            return StringExtension.PrettyPrint(input);
        }
    }
}
