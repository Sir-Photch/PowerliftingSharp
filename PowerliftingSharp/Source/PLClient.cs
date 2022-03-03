using System.Globalization;
using PowerliftingSharp.Util;
using PowerliftingSharp.Types;

namespace PowerliftingSharp
{
    /// <summary>
    /// Main client to retrieve data with
    /// </summary>
    public class PLClient : IDisposable
    {
        #region private fields
        private bool _disposed = false;
        private readonly HttpClient _httpClient = new();
        private readonly SemaphoreSlim _clientSemaphore = new(1);
        #endregion

        /// <summary>
        /// Queries lifter data with given <paramref name="identifier"/>. 
        /// </summary>
        /// <param name="identifier">Uniquie identifier of lifter</param>
        /// <param name="token">Optional token to cancel operation</param>
        /// <returns><see cref="Athlete"/>-data</returns>
        /// <exception cref="ObjectDisposedException">Instance is disposed</exception>
        /// <exception cref="DeserializeException">An internal parse-error has occurred.</exception>
        /// <exception cref="ArgumentException">Request for given <paramref name="identifier"/> was not successful</exception>
        /// <exception cref="TaskCanceledException">Operation was canceled via <paramref name="token"/></exception>
        public async Task<Athlete> GetAthleteByIdentifierAsync(string identifier, CancellationToken token = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PLClient));

            string csvUrl = GetLifterCsvUrl(identifier);

            using Stream? stream = await RequestStreamAsync(csvUrl, token);

            if (stream is null)
                throw new ArgumentException($"Request for {identifier} was not successful!");

            using CsvLineParser parser = new(stream);

            var rows = parser.EnumerateRows().Skip(1).ToList(); // first line is formatting

            if (rows.Empty())
                throw new ArgumentException($"Athlete {identifier} did not contain valid data.");

            if (rows.Any(row => row is null))
                throw new DeserializeException("Internal error; could not deserialize .csv-data");

            return FromFieldRows(rows, identifier);
        }

        /// <summary>
        /// Queries unique lifter identifier given (parts) of lifters full name.
        /// </summary>
        /// <param name="fullName">Lifters full name</param>
        /// <param name="token">Optional token to cancel operation</param>
        /// <returns>Best match including found name and unique identifier, or <c>null</c>, when query was not successful.</returns>
        /// <exception cref="ObjectDisposedException">Instance is disposed</exception>
        /// <exception cref="DeserializeException">An internal parse-error has occurred.</exception>
        public async Task<(string FoundName, string Identifier)?> QueryName(string fullName, CancellationToken token = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(_httpClient));

            string nextIndexUrl = GetNameQueryUrl(fullName);

            using Stream? indexResponseStream = await RequestStreamAsync(nextIndexUrl, token);

            if (indexResponseStream is null)
                return null;

            JsonToXmlConverter converter = new();
            converter.ReadStream(indexResponseStream);
            int? nextIndex = converter.GetValue<int>("next_index");

            if (!nextIndex.HasValue)
                return null;

            string entryQueryUrl = GetEntryQueryUrl(nextIndex.Value);

            using Stream? entryResponseStream = await RequestStreamAsync(entryQueryUrl, token);

            if (entryResponseStream is null)
                return null;

            converter.ReadStream(entryResponseStream);

            var items = converter["rows"]?.Element("item")?.Elements("item");

            if (items is null)
                throw new DeserializeException("Could not deserialize json; OpenPowerliftingAPI probably has changed");

            string lifterName = items.ElementAt(2).Value;
            string identifier = items.ElementAt(3).Value;

            return (lifterName, identifier);
        }

        /// <summary>
        /// Disposes underlying http-client.<br/>
        /// Successive calls to client will cause <see cref="ObjectDisposedException"/>s to be thrown.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        #region private methods

        private async Task<Stream?> RequestStreamAsync(string url, CancellationToken token)
        {
            await _clientSemaphore.WaitAsync(token);

            using HttpRequestMessage message = new(HttpMethod.Get, url);
            HttpResponseMessage response = await _httpClient.SendAsync(message, token);

            _clientSemaphore.Release();

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStreamAsync(token);
        }


        /* Name,Sex,Event,Equipment,Age,AgeClass,BirthYearClass,Division,BodyweightKg,WeightClassKg,
         * Squat1Kg,Squat2Kg,Squat3Kg,Squat4Kg,Best3SquatKg,
         * Bench1Kg,Bench2Kg,Bench3Kg,Bench4Kg,Best3BenchKg,
         * Deadlift1Kg,Deadlift2Kg,Deadlift3Kg,Deadlift4Kg,Best3DeadliftKg,
         * TotalKg,Place,Dots,Wilks,Glossbrenner,Goodlift,Tested,
         * Country,State,Federation,ParentFederation,Date,MeetCountry,MeetState,MeetTown,MeetName
         */

        private static string GetLifterCsvUrl(string lifterId)
        {
            return $"https://www.openpowerlifting.org/u/{lifterId}/csv";
        }

        private static string GetNameQueryUrl(string fullName)
        {
            return $"https://www.openpowerlifting.org/api/search/rankings?q={fullName}&start=0";
        }

        private static string GetEntryQueryUrl(int nextIndex)
        {
            return $"https://www.openpowerlifting.org/api/rankings?start={nextIndex}&end={nextIndex}&lang=en&units=kg";
        }

        private static Athlete FromFieldRows(IEnumerable<string[]> fieldRows, string lifterIdentifier) => new()
        {
            FullName = fieldRows.First()[0],
            Identifier = lifterIdentifier,
            Sex = Enum.Parse<Sex>(fieldRows.First()[1]),
            Meets = fieldRows.Select(fr => FromFieldRow(fr)).ToHashSet()
        };

        private static Meet FromFieldRow(string[] fieldRow)
        {
            float?[] allAttempts = new float?[12];

            for (int i = 0; i < 12; i++)
            {
                int offset = i < 4 ? 10 : i < 8 ? 11 : 12; // remove this

                allAttempts[i] = !string.IsNullOrEmpty(fieldRow[i + offset])
                    ? float.Parse(fieldRow[i + offset], NumberStyles.Float, CultureInfo.InvariantCulture)
                    : null;
            }

            Attempts attempts = new()
            {
                Squat = allAttempts[..4],
                Bench = allAttempts[4..8],
                Deadlift = allAttempts[8..]
            };

            (uint Kg, bool open)? weightClass = null;
            if (!string.IsNullOrEmpty(fieldRow[9]))
                weightClass = (fieldRow[9].EndsWith('+') ? uint.Parse(fieldRow[9][0..^1]) : uint.Parse(fieldRow[9]), fieldRow[9].EndsWith('+'));


            (PlaceType type, uint? rank) place;
            if (uint.TryParse(fieldRow[26], out uint rnk))
                place = (PlaceType.Ranked, rnk);
            else
                place = (PtFromString(fieldRow[26]), null);

            return new()
            {
                Event = Enum.Parse<Event>(fieldRow[2]),
                Equipment = EqFromString(fieldRow[3]),
                Age = string.IsNullOrEmpty(fieldRow[4]) ? null : float.Parse(fieldRow[4], NumberStyles.Float, CultureInfo.InvariantCulture),
                AgeClass = NullWhenEmpty(fieldRow[5]),
                BirthYearClass = NullWhenEmpty(fieldRow[6]),
                Division = DivFromString(fieldRow[7]),
                BodyweightKg = string.IsNullOrEmpty(fieldRow[8]) ? null : float.Parse(fieldRow[8], NumberStyles.Float, CultureInfo.InvariantCulture),
                WeightClassKg = weightClass,
                Attempts = attempts,
                Place = place,
                Dots = string.IsNullOrEmpty(fieldRow[27]) ? null : float.Parse(fieldRow[27], NumberStyles.Float, CultureInfo.InvariantCulture),
                Wilks = string.IsNullOrEmpty(fieldRow[28]) ? null : float.Parse(fieldRow[28], NumberStyles.Float, CultureInfo.InvariantCulture),
                Glossbrenner = string.IsNullOrEmpty(fieldRow[29]) ? null : float.Parse(fieldRow[29], NumberStyles.Float, CultureInfo.InvariantCulture),
                Goodlift = string.IsNullOrEmpty(fieldRow[30]) ? null : float.Parse(fieldRow[30], NumberStyles.Float, CultureInfo.InvariantCulture),
                Tested = fieldRow[31] is "Yes",
                Country = NullWhenEmpty(fieldRow[32]),
                State = NullWhenEmpty(fieldRow[33]),
                Federation = fieldRow[34],
                ParentFederation = NullWhenEmpty(fieldRow[35]),
                Date = DateOnly.Parse(fieldRow[36]),
                MeetCountry = fieldRow[37],
                MeetState = NullWhenEmpty(fieldRow[38]),
                MeetTown = NullWhenEmpty(fieldRow[39]),
                MeetName = fieldRow[40]
            };
        }

        private static string? NullWhenEmpty(string src) => string.IsNullOrEmpty(src) ? null : src;

        private static Division? DivFromString(string src) => src switch
        {
            "Sub-Juniors" => Division.SubJunior,
            "Juniors" => Division.Junior,
            "Seniors" => Division.Senior,
            "Masters 1" => Division.Masters1,
            "Masters 2" => Division.Masters2,
            "Masters 3" => Division.Masters3,
            "Masters 4" => Division.Masters4,
            _ => null
        };

        private static Equipment EqFromString(string src) => src switch
        {
            "Raw" => Equipment.Raw,
            "Wraps" => Equipment.Wraps,
            "Single-ply" => Equipment.SinglePly,
            "Multi-ply" => Equipment.MultiPly,
            "Unlimited" => Equipment.Unlimited,
            "Straps" => Equipment.Straps,
            _ => throw new NotImplementedException($"Equipment '{src}' is not implemented!")
        };

        private static PlaceType PtFromString(string src) => src switch
        {
            "G" => PlaceType.Guest,
            "DQ" => PlaceType.Disqualified,
            "DD" => PlaceType.DopingDisqualification,
            "NS" => PlaceType.NoShow,
            _ => throw new NotImplementedException($"Place type '{src}' is not implemented!")
        };

        #endregion
    }
}
