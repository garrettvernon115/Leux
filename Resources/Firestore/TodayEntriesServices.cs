using Firebase.Auth;
using Leux.Resources.Firestore;

namespace Leux
{
    public class TodayEntriesService
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirestoreRestClient _rest;

        public TodayEntriesService(FirebaseAuthClient auth, FirestoreRestClient rest)
        {
            _auth = auth;
            _rest = rest;
        }

        public async Task<List<ExpenseItem>> GetTodayAsync(string userId, TimeZoneInfo? tz = null)
        {
            var token = _auth.User == null ? null : await _auth.User.GetIdTokenAsync(false);
            if (token == null) return new();

            tz ??= TimeZoneInfo.Local;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today, tz);
            var endUtc = startUtc.AddDays(1);

            var query = new
            {
                from = new[] { new { collectionId = "entries" } },
                where = new
                {
                    compositeFilter = new
                    {
                        op = "AND",
                        filters = new object[]
                        {
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "occurredAt" },
                                    op = "GREATER_THAN_OR_EQUAL",
                                    value = new { timestampValue = startUtc.ToString("o") }
                                }
                            },
                            new
                            {
                                fieldFilter = new
                                {
                                    field = new { fieldPath = "occurredAt" },
                                    op = "LESS_THAN",
                                    value = new { timestampValue = endUtc.ToString("o") }
                                }
                            }
                        }
                    }
                },
                orderBy = new[] { new { field = new { fieldPath = "occurredAt" }, direction = "DESCENDING" } }
            };

            var docs = await _rest.RunQueryAsync($"users/{userId}", query, token);
            return docs.Select(d =>
            {
                var f = d.Fields;
                return new ExpenseItem
                {
                    Id = d.Id,
                    Category = f.TryGetValue("category", out var c) ? (string)c : "Other",
                    Description = f.TryGetValue("description", out var ds) ? (string)ds : "",
                    Amount = f.TryGetValue("amount", out var a) ? ToDouble(a) : 0d,
                    OccurredAt = f.TryGetValue("occurredAt", out var t) && t is DateTime dt
                        ? dt.ToLocalTime() : DateTime.Now
                };
            }).ToList();
        }

        private static double ToDouble(object v)
        {
            if (v is double d) return d;
            if (v is long l) return l;
            if (v is int i) return i;
            return double.TryParse(v?.ToString(), out var x) ? x : 0d;
        }
    }
}
