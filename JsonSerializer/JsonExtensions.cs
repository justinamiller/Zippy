using JsonSerializer.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace JsonSerializer
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
           
            return Serializer.SerializeObject(instance);
        }

        internal static string GetBadJson(object instance, Exception ex)
        {
            using (var writer = new JsonTextWriter())
            {
                writer.WriteStartObject();
                if (instance != null)
                {
                    writer.WriteProperty("_JsonError_Object", instance.GetType().FullName);
                }
                writer.WriteProperty("_JsonError_Message", ex.Message);
                writer.WriteProperty("_JsonError_Details", ex.ToString());
                writer.WriteEndObject();

                return writer.ToString();
            }
        }

        /// <summary>
        /// List of namespaces where we do not Serialize
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        [SuppressMessage("brain-overload", "S1541")]
        [SuppressMessage("brain-overload", "S1067")]
        private static bool CanSerializeComplexObject(object instance)
        {
            string typeFullName = instance.GetType().FullName;

            //filter out unknown type
            if (typeFullName == null)
            {
                return false;
            }

            //filter out by namespace & Types
            else if (typeFullName.FastStartsWith("System."))
            {
                if (typeFullName.FastStartsWith("System.Reflection")
                    || typeFullName.FastStartsWith("System.Runtime")
                    || typeFullName.FastStartsWith("System.Threading")
                    || typeFullName.FastStartsWith("System.Security")
                    || typeFullName == "System.RuntimeType"
                    || typeFullName == "System.AppDomain"
                    )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
