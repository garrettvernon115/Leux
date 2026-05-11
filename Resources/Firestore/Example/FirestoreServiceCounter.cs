using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Models;
using System.Diagnostics;

namespace Leux.Resources.Firestore.Example
{
    public class FirestoreServiceCounter : IFireStoreServiceCounter
    {
        private const string Path = "app_data/counter";
        private readonly FirebaseAuthClient _auth;
        private readonly FirestoreRestClient _rest;

        public FirestoreServiceCounter(FirebaseAuthClient auth, FirestoreRestClient rest)
        {
            _auth = auth;
            _rest = rest;
        }

        private async Task<string?> Token() =>
            _auth.User == null ? null : await _auth.User.GetIdTokenAsync(false);

        public async Task<CounterData> GetCounterAsync()
        {
            var token = await Token();
            if (token == null) return new CounterData();
            try
            {
                var doc = await _rest.GetDocumentAsync(Path, token);
                if (doc == null) return new CounterData();
                int count = doc.TryGetValue("count", out var c) ? (int)ToLong(c) : 0;
                var lastUpdated = doc.TryGetValue("lastUpdated", out var lu) && lu is DateTime dt
                    ? Timestamp.FromDateTime(DateTime.SpecifyKind(dt, DateTimeKind.Utc))
                    : Timestamp.GetCurrentTimestamp();
                return new CounterData(count) { LastUpdated = lastUpdated };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting counter: {ex.Message}");
                return new CounterData();
            }
        }

        public async Task<bool> UpdateCounterAsync(int count)
        {
            var token = await Token();
            if (token == null) return false;
            return await _rest.SetDocumentAsync(Path, new Dictionary<string, object?>
            {
                ["count"] = count,
                ["lastUpdated"] = DateTime.UtcNow
            }, token);
        }

        public async Task<bool> IncrementCounterAsync()
        {
            var token = await Token();
            if (token == null) return false;
            try
            {
                var doc = await _rest.GetDocumentAsync(Path, token);
                int current = doc != null && doc.TryGetValue("count", out var c) ? (int)ToLong(c) : 0;
                return await UpdateCounterAsync(current + 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error incrementing counter: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetCounterAsync() => await UpdateCounterAsync(0);

        private static long ToLong(object v)
        {
            if (v is long l) return l;
            if (v is int i) return i;
            if (v is double d) return (long)d;
            return long.TryParse(v?.ToString(), out var x) ? x : 0L;
        }
    }
}
