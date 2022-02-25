using System.Runtime.Serialization;

namespace PowerliftingSharp.Util;

public class DeserializeException : Exception
{
    internal DeserializeException() : base() { }
    internal DeserializeException(string message) : base(message) { }
    internal DeserializeException(string message, Exception innerException) : base(message, innerException) { }
    protected DeserializeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
