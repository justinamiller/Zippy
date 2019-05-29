using JsonSerializer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static JsonSerializer.Utility.ConvertUtils;

namespace JsonSerializer
{
    public abstract class JsonWriter : IDisposable
    {
        //used to format property names for elastic.
        public bool IsElasticSearchReady { get; set; } = true;


        internal virtual int Length
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// indicate if json is valid format.
        /// </summary>
        public virtual bool Valid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// output json to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Empty;
        }

        public virtual void Dispose()
        {
            return;
        }

        //public JsonWriter(TextWriter writer)
        //{
        //    _textWriter = writer;
        //}

        protected JsonWriter()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteStartArray()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteStartObject()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteEndArray()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteEndObject()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteComma()
        {
        }

        internal virtual void WriteValue(IEnumerable enumerable)
        {
        }

        public virtual void WriteValue(object value)
        {
        }

        internal virtual void WriteObjectValue(object value, ConvertUtils.TypeCode typeCode)
        {
        }

        /// <summary>
        /// call for json string
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteRawValue(string value, bool doValidate)
        {
        }

        public virtual void WriteProperty(string name, string value)
        {
        }


        public virtual void WriteProperty(string name, Guid value)
        {
        }

        internal virtual void WriteValue(Guid value)
        {
        }

        public virtual void WriteProperty(string name, bool value)
        {
        }

        internal virtual void WriteValue(bool value)
        {
        }

        public virtual void WriteProperty(string name, int value)
        {
        }

        public virtual void WriteProperty(string name, char value)
        {
        }

        internal virtual void WriteValue(int value)
        {
        }

        public virtual void WriteProperty(string name, uint value)
        {
        }

        internal virtual void WriteValue(uint value)
        {
        }

        public virtual void WriteProperty(string name, sbyte value)
        {
        }

        internal virtual void WriteValue(sbyte value)
        {
        }

        public virtual void WriteProperty(string name, byte value)
        {
        }

        internal virtual void WriteValue(byte value)
        {
        }

        public virtual void WriteProperty(string name, short value)
        {
        }

        internal virtual void WriteValue(short value)
        {
        }

        public virtual void WriteProperty(string name, ushort value)
        {
        }

        internal virtual void WriteValue(ushort value)
        {
        }

        public virtual void WriteProperty(string name, double value)
        {
        }

        internal virtual void WriteValue(double value)
        {
        }

        internal virtual void WriteValue(char value)
        {
        }

        public virtual void WriteProperty(string name, long value)
        {
        }

        internal virtual void WriteValue(long value)
        {
        }

        internal virtual void WriteValue(ulong value)
        {
        }

        internal virtual void WriteValue(float value)
        {
        }

        internal virtual void WriteValue(decimal value)
        {

        }

        internal virtual void WriteValue(Enum value)
        {
        }

        public virtual void WriteProperty(string name, Uri value)
        {

        }

        internal virtual void WriteValue(Uri value)
        {

        }

        public virtual void WriteProperty(string name, DateTime value)
        {
        }

        internal virtual void WriteValue(DateTime value)
        {
        }

        public virtual void WriteProperty(string name, TimeSpan value)
        {
        }

        internal virtual void WriteValue(TimeSpan value)
        {
        }

        public virtual void WriteProperty(string name, object value)
        {
        }

        public virtual void WriteProperty(string name, IDictionary<string, string> values)
        {
        }

        internal virtual void WriteValue(IDictionary<string, string> values)
        {
        }

        public virtual void WriteProperty(string name, object[] values)
        {

        }



        /// <summary>
        /// requires to be json compliant
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public virtual void WritePropertyName(string name, bool escape = true)
        {
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteNull()
        {
        }

        /// <summary>
        /// use pointers to improve performance
        /// </summary>
        /// <param name="str"></param>
        public virtual void WriteValue(string str)
        {
         
        }

    }
}
