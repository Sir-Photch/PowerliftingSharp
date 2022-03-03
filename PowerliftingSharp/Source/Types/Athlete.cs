namespace PowerliftingSharp.Types;

public readonly struct Athlete
{
    public string FullName { get; internal init; }
    public string Identifier { get; internal init; }
    public Sex Sex { get; internal init; }
    public IReadOnlySet<Meet> Meets { get; internal init; }

    public override string ToString()
    {
        return $"{FullName}, {Meets.Count} Meets";
    }
}
