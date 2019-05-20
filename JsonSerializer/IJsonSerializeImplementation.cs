using System;
using System.Collections.Generic;
using System.Text;

namespace JsonSerializer
{
    /// <summary>
    /// when this is Implementation then object can handle it's own serialization.
    /// </summary>
    public interface IJsonSerializeImplementation
    {
        string SerializeAsJson();
    }
}
