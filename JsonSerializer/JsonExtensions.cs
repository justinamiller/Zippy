using Zippy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
        public static string ConvertToJson(this object instance)
        {
            if (instance == null || DBNull.Value.Equals(instance))
            {
                return null;
            }
           
            return JSON.SerializeObjectToString(instance);
        }

        public static string BeautifyJson(string input)
        {
            return StringExtension.PrettyPrint(input);
        }
    }
}
