using Microsoft.VisualBasic.FileIO;
using PowerliftingSharp.Types;
using System.Text;
using System.Globalization;

namespace PowerliftingSharp.WebClient
{
    public class PLClient : IPLClient, IDisposable
    {
        #region private fields
        private bool _disposed = false;
        private readonly HttpClient _httpClient;
        #endregion

        public PLClient()
        {
            _httpClient = new();
        }

        public async Task<Lifter?> GetLifterByIdentifierAsync(string identifier)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(_httpClient));

            string csvUrl = GetLifterCsvUrl(identifier);

            using HttpRequestMessage request = new(HttpMethod.Get, csvUrl);
            using HttpResponseMessage response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();

            using TextFieldParser parser = new(responseStream);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.ReadLine(); // skip first line

            List<string[]> fieldRows = new();
            while (!parser.EndOfData)
                fieldRows.Add(parser.ReadFields());

            if (fieldRows.Count is 0)
                return null;

            return FromFieldRows(fieldRows, identifier);
        }

        public Task<string?> GetLifterIdentifier(string fullName)
        {
            throw new NotImplementedException();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _httpClient.Dispose();
            }

            _disposed = true;
        }

        ~PLClient() => Dispose(false);
        #endregion

        #region private methods

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

        private static Lifter FromFieldRows(List<string[]> fieldRows, string lifterIdentifier) => new()
        {
            FullName = fieldRows[0][0],
            Identifier = lifterIdentifier,
            Sex = Enum.Parse<Sex>(fieldRows[0][1]),
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
                place = (Enum.Parse<PlaceType>(fieldRow[26]), null);

            return new()
            {
                Event = Enum.Parse<Event>(fieldRow[2]),
                Equipment = Enum.Parse<Equipment>(fieldRow[3]),
                Age = string.IsNullOrEmpty(fieldRow[4]) ? null : float.Parse(fieldRow[4], NumberStyles.Float, CultureInfo.InvariantCulture),
                AgeClass = NullWhenEmpty(fieldRow[5]),
                BirthYearClass = NullWhenEmpty(fieldRow[6]),
                Division = FromString(fieldRow[7]),
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

        private static Division? FromString(string src) => src switch
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

        #endregion
    }
}
