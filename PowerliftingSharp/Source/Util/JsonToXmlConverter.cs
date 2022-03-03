using System.Xml.Linq;
using System.Runtime.Serialization.Json;

namespace PowerliftingSharp.Util;

internal class JsonToXmlConverter
{
    private XDocument? _doc;

    internal void ReadStream(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanRead)
            throw new ArgumentException("Not readable", nameof(stream));

        using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, new());

        _doc = XDocument.Load(reader);
    }

    internal XElement? this[XName xName]
    {
        get
        {
            if (_doc is null)
                throw new InvalidOperationException("No stream set");

            if (xName is null)
                throw new ArgumentNullException(nameof(xName));

            return _doc?.Root?.Element(xName);
        }
    }

    internal T? GetValue<T>(XName xName) where T : struct
    {
        if (_doc is null)
            throw new InvalidOperationException("No stream set");

        if (xName is null)
            throw new ArgumentNullException(nameof(xName));

        string? element = _doc?.Root?.Element(xName)?.Value;

        if (string.IsNullOrWhiteSpace(element))
            return null;

        return (T?)Convert.ChangeType(element, typeof(T));
    }
}
