using System;

namespace Zippy
{
    /// <summary>
    /// Alternative to using [DataMember] and [IgnoreDataMember], for 
    /// when their use isn't possible.
    /// 
    /// When applied to a property or field, allows configuration
    /// of the name (de)serialized, whether to (de)serialize at all,
    /// and the primitive type to treat an enum type as.
    /// 
    /// Takes precedence over [DataMember] and [IgnoreDataMember].
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ZippyDirectiveAttribute : Attribute
    {
        /// <summary>
        /// If true, the decorated member will not be serialized or deserialized.
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// If non-null, the decorated member's name in serialization will match
        /// the value of Name.
        /// 
        /// When deserializing, Name is used to map to a member.  This mapping is
        /// case sensitive.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Create a new SwiftDirectiveAttribute
        /// </summary>
        public ZippyDirectiveAttribute() { }

        /// <summary>
        /// Create a new SwiftDirectiveAttribute, with a name override.
        /// </summary>
        public ZippyDirectiveAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create a new SwiftDirectiveAttribute, optionally ignoring the decorated member.
        /// </summary>
        public ZippyDirectiveAttribute(bool ignore)
        {
            Ignore = ignore;
        }
    }
}
