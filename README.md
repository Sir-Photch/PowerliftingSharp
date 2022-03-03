<p align="center">
  <img src="PowerliftingSharp.png" alt="PowerliftingSharp" width=200 class="center"/>
</p>

# Lightweight! C# Wrapper for OpenPowerlifting API

Built with .NET 6.0 and available on [nuget](https://www.nuget.org/packages/PowerliftingSharp/1.1.0)


Currently, querying unique identifiers of athletes and returning their recorded data is supported.
Feel free to fork this repo or open an issue if you miss a feature!

## How to get started

```csharp
using PowerliftingSharp;
using PowerliftingSharp.Types;

using PLClient client = new();

string nameToBeFound = "Andrey Malanichev";

(string foundName, string identifier)? query = await client.QueryName(nameToBeFound);

if (query is null || query.Value.foundName != nameToBeFound)
  return;
  
Athlete? andrey;
try
{
  andrey = await client.GetAthleteByIdentifierAsync(query.Value.identifier);
}
catch (Exception e)
{
  // request was not successful, or, an internal error has occurred.
}
```

For reference, see [OPL data service](https://openpowerlifting.gitlab.io/opl-csv/) and the official [Gitlab repo](https://gitlab.com/openpowerlifting/opl-data).
