using Microsoft.VisualBasic.FileIO;

namespace PowerliftingSharp.Util;

internal class CsvLineParser : IDisposable
{
    private readonly TextFieldParser _parser;
    private bool _disposed = false;

    internal CsvLineParser(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        if (!stream.CanRead)
            throw new ArgumentException("Not readable", nameof(stream));

        _parser = new(stream);
        _parser.TextFieldType = FieldType.Delimited;
        _parser.SetDelimiters(",");
    }

    internal IEnumerable<string[]?> EnumerateRows()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CsvLineParser));        

        while (!_parser.EndOfData) yield return _parser.ReadFields();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _parser.Close();

        _disposed = true;
    }

}
