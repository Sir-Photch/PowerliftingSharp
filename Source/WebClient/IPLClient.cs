using PowerliftingSharp.Types;

namespace PowerliftingSharp.WebClient;

public interface IPLClient
{
    public Task<Lifter?> GetLifterByIdentifierAsync(string identifier);
    public Task<string?> GetLifterIdentifier(string fullName);
}
