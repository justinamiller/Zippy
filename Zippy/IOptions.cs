namespace Zippy
{
    public interface IOptions
    {
 //       int MaxJsonLength { get; set; }

            /// <summary>
            /// depth to drill into object graph
            /// </summary>
        int RecursionLimit { get; set; }

        /// <summary>
        /// Skip property when value is null
        /// </summary>
        bool ExcludeNulls { get; set; }

        DateHandler DateHandler { get; set; }

        /// <summary>
        ///  if HTML entity chars [&gt; &lt; &amp; = '] should be escaped as "\uXXXX".
        /// </summary>
        bool EscapeHtmlChars { get; set; }

        /// <summary>
        /// whether or not to include whitespace and newlines for ease of reading
        /// </summary>
        bool PrettyPrint { get; set; }

        /// <summary>
        /// Text case to use for property names
        /// </summary>
        TextCase TextCase { get; set; }

        /// <summary>
        /// What is done when unable to serialize field or property.
        /// </summary>
        SerializationErrorHandling SerializationErrorHandling { get; set; }
    }

    public enum SerializationErrorHandling
    {
        ReportValueAsNull=0,
        SkipProperty=1,
        ThrowException=2
    }

    public enum DateHandler
    {
        TimestampOffset = 0,
        ISO8601 = 1,
        ISO8601DateOnly = 2,
        ISO8601DateTime = 3,
        RFC1123 = 4
    }

    public enum TextCase
    {
        /// <summary>
        /// If unspecified use as is.
        /// </summary>
        Default,
        /// <summary>
        /// camelCase
        /// </summary>
        CamelCase,
        /// <summary>
        /// snake_case
        /// </summary>
        SnakeCase,
    }
}
