namespace Zippy
{
    public interface IOptions
    {
 //       int MaxJsonLength { get; set; }

        int RecursionLimit { get; set; }

        //bool ShouldExcludeNulls { get; set; }

        /// <summary>
        /// Addtional encoding for supporting direct elastic search write.
        /// </summary>
     //   bool IsElasticSearchReady { get; set; }

        DateHandler DateHandler { get; set; }

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
        DCJSCompatible = 1,
        ISO8601 = 2,
        ISO8601DateOnly = 3,
        ISO8601DateTime = 4,
        RFC1123 = 5
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
