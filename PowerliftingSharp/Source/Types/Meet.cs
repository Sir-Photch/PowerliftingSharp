namespace PowerliftingSharp.Types;

public readonly struct Meet : IEquatable<Meet>
{
    public Event Event { get; internal init; }
    public Equipment Equipment { get; internal init; }
    public float? Age { get; internal init; }
    public string? AgeClass { get; internal init; }
    public string? BirthYearClass { get; internal init; }
    public Division? Division { get; internal init; }
    public float? BodyweightKg { get; internal init; }
    public (uint Kg, bool open)? WeightClassKg { get; internal init; }
    public Attempts Attempts { get; internal init; }
    public (PlaceType Type, uint? Rank) Place { get; internal init; }
    public float? Dots { get; internal init; }
    public float? Wilks { get; internal init; }
    public float? Glossbrenner { get; internal init; }
    public float? Goodlift { get; internal init; }
    public bool Tested { get; internal init; }
    public string? Country { get; internal init; }
    public string? State { get; internal init; }
    public string Federation { get; internal init; }
    public string? ParentFederation { get; internal init; }
    public DateOnly Date { get; internal init; }
    public string MeetCountry { get; internal init; }
    public string? MeetState { get; internal init; }
    public string? MeetTown { get; internal init; }
    public string MeetName { get; internal init; }

    public override string ToString()
    {
        return $"{Federation}: {MeetName}, {Date:y}, Place: {(Place.Rank?.ToString() ?? Place.Type.ToString())}";
    }

    #region ops
    public override bool Equals(object? obj)
    {
        return obj is Meet meet && Equals(meet);
    }

    public bool Equals(Meet other)
    {
        return Event == other.Event &&
               Equipment == other.Equipment &&
               Age == other.Age &&
               AgeClass == other.AgeClass &&
               BirthYearClass == other.BirthYearClass &&
               Division == other.Division &&
               BodyweightKg == other.BodyweightKg &&
               WeightClassKg == other.WeightClassKg &&
               Attempts.Equals(other.Attempts) &&
               Place.Equals(other.Place) &&
               Dots == other.Dots &&
               Wilks == other.Wilks &&
               Glossbrenner == other.Glossbrenner &&
               Goodlift == other.Goodlift &&
               Tested == other.Tested &&
               Country == other.Country &&
               State == other.State &&
               Federation == other.Federation &&
               ParentFederation == other.ParentFederation &&
               Date.Equals(other.Date) &&
               MeetCountry == other.MeetCountry &&
               MeetState == other.MeetState &&
               MeetName == other.MeetName;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Event);
        hash.Add(Equipment);
        hash.Add(Age);
        hash.Add(AgeClass);
        hash.Add(BirthYearClass);
        hash.Add(Division);
        hash.Add(BodyweightKg);
        hash.Add(WeightClassKg);
        hash.Add(Attempts);
        hash.Add(Place);
        hash.Add(Dots);
        hash.Add(Wilks);
        hash.Add(Glossbrenner);
        hash.Add(Goodlift);
        hash.Add(Tested);
        hash.Add(Country);
        hash.Add(State);
        hash.Add(Federation);
        hash.Add(ParentFederation);
        hash.Add(Date);
        hash.Add(MeetCountry);
        hash.Add(MeetState);
        hash.Add(MeetName);
        return hash.ToHashCode();
    }

    public static bool operator ==(Meet left, Meet right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Meet left, Meet right)
    {
        return !(left == right);
    }
    #endregion
}
