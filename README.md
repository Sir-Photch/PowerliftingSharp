# Lightweight C# Wrapper for OpenPowerlifting API

Available on nuget: https://www.nuget.org/packages/PowerliftingSharp/1.1.0

## how to get started

```csharp
using PowerliftingSharp;
using PowerliftingSharp.Types;

using PLClient client = new();

string nameToBeFound = "Andrey Malanichev";

(string foundName, string identifier)? query = await client.QueryName(nameToBeFound);

if (query is null || query.Value.foundName != nameToBeFound)
  return;
  
Lifter? lifter;
try
{
  lifter = await client.GetLifterByIdentifierAsync(query.Value.identifier);
}
catch (Exception e)
{
  // request was not successful, or, an internal error has occurred.
}
```

For reference, see https://gitlab.com/openpowerlifting/opl-data / https://www.openpowerlifting.org/
