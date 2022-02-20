using System.Linq;

namespace PowerliftingSharp.Types;

public readonly struct Attempts : IEquatable<Attempts>
{
    public IReadOnlyCollection<float?>? Squat { get; internal init; }
    public IReadOnlyCollection<float?>? Bench { get; internal init; }
    public IReadOnlyCollection<float?>? Deadlift { get; internal init; }
    public IReadOnlyCollection<float?> Best => new float?[] { Squat?.Max(), Bench?.Max(), Deadlift?.Max() };
    public float? Total => Best.Sum();

    #region ops
    public override bool Equals(object? obj)
    {
        return obj is Attempts attempts && Equals(attempts);
    }

    public bool Equals(Attempts other)
    {
        return EqualityComparer<IReadOnlyCollection<float?>?>.Default.Equals(Squat, other.Squat) &&
               EqualityComparer<IReadOnlyCollection<float?>?>.Default.Equals(Bench, other.Bench) &&
               EqualityComparer<IReadOnlyCollection<float?>?>.Default.Equals(Deadlift, other.Deadlift) &&
               EqualityComparer<IReadOnlyCollection<float?>>.Default.Equals(Best, other.Best) &&
               Total == other.Total;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Squat, Bench, Deadlift, Best, Total);
    }

    public static bool operator ==(Attempts left, Attempts right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Attempts left, Attempts right)
    {
        return !(left == right);
    }
    #endregion
}
