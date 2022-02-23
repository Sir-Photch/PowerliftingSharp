using System.Runtime.Serialization;

namespace PowerliftingSharp.Util;

public class DeserializeException : Exception
{
    public DeserializeException() : base() { }
    public DeserializeException(string message) : base(message) { }
    public DeserializeException(string message, Exception innerException) : base(message, innerException) { }
    protected DeserializeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
