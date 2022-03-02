# Lightweight C# Wrapper for OpenPowerlifting API

## how to get started

```csharp
using PowerliftingSharp;
using PowerliftingSharp.Types;

using PLClient client = new();

string nameToBeFound = "Andrey Malanichev";

(string foundName, string identifier) = await client.QueryName(nameToBeFound);

if (foundName != nameToBeFound)
  Environment.Exit(1);
  
Lifter? lifter;
try
{
  lifter = await client.GetLifterByIdentifierAsync(identifier);
}
catch (Exception e)
{
  // API error
}
```

For reference, see https://gitlab.com/openpowerlifting/opl-data / https://www.openpowerlifting.org/
