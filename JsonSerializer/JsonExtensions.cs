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
            else if (instance is IJsonSerializeImplementation)
            {
                return ((IJsonSerializeImplementation)instance).SerializeAsJson();
            }
            else if (instance is string)
            {
                return SerializeStringObject((string)instance);
            }
            else if (instance is IDictionary<string, string>)
            {
                return SerializeIDictionaryObject((IDictionary<string, string>)instance);
            }
            else if (instance.GetType().IsPrimitive)
            {
                return SerializePrimitiveObject(instance);
            }

            return SerializeComplexObject(instance);
        }

        private static string SerializeIDictionaryObject(IDictionary<string, string> instance)
        {
            using (var writer = new JsonWriter())
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<string, string> item in instance)
                {
                    if (!item.Key.IsNullOrWhiteSpace())
                    {
                        writer.WriteProperty(item.Key, item.Value, false);
                    }
                }
                writer.WriteEndObject();

                return writer.ToString();
            }
        }

        private static string SerializePrimitiveObject(object instance)
        {
            using (var writer = new JsonWriter())
            {
                writer.WriteStartObject();
                writer.WriteProperty(string.Concat("_", instance.GetType().Name), instance.ToString(), false);
                writer.WriteEndObject();
                return writer.ToString();
            }
        }


        private static string SerializeXmlObject(System.Xml.XmlNode xml)
        {
            using (var writer = new JsonWriter())
            {
                writer.WriteStartObject();
                writer.WriteProperty("_XmlNode", xml.OuterXml);
                writer.WriteEndObject();
                return writer.ToString();
            }
        }


        private static string SerializeStringObject(string str)
        {
            //check if json format
            if (JsonWriter.ValidJsonFormat(str, true))
            {
                return str;
            }//end json valid check

            using (var writer = new JsonWriter())
            {
                writer.WriteStartObject();
                writer.WriteProperty("_String", str);
                writer.WriteEndObject();
                return writer.ToString();
            }
        }

        private static string SerializeComplexObject(object instance)
        {
            if (instance is System.Xml.XmlNode)
            {
                return SerializeXmlObject(((System.Xml.XmlNode)instance));
            }
            //check if we can serialize complex object.
            else if (!CanSerializeComplexObject(instance))
            {
                return GetBadJson(instance, new Exception("Can not Serialize."));
            }

            try
            {
                return JsonWriter.SerializeCustomObject(instance);
            }
            catch (Exception ex)
            {
                return GetBadJson(instance, ex);
            }
        }

        public static string GetBadJson(object instance, Exception ex)
        {
            using (var writer = new JsonWriter())
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
