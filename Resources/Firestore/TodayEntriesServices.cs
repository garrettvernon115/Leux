using Google.Cloud.Firestore;

namespace Leux;

public class TodayEntriesService
{
    public async Task<List<ExpenseItem>> GetTodayAsync(string userId, TimeZoneInfo? tz = null)
    {
        if (!Resources.Firestore.FirestoreDatabase.IsInitialized)
            await Resources.Firestore.FirestoreDatabase.InitializeAsync();

        var db = Resources.Firestore.FirestoreDatabase.Database;

        tz ??= TimeZoneInfo.Local;
        var startLocal = DateTime.Today;
        var endLocal = startLocal.AddDays(1);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
   
        var q = db.Collection("users").Document(userId).Collection("entries")
            .WhereGreaterThanOrEqualTo("occurredAt", Timestamp.FromDateTime(DateTime.SpecifyKind(startUtc, DateTimeKind.Utc)))
            .WhereLessThan("occurredAt", Timestamp.FromDateTime(DateTime.SpecifyKind(endUtc, DateTimeKind.Utc)))
            .OrderByDescending("occurredAt");

        var snap = await q.GetSnapshotAsync();

        var list = new List<ExpenseItem>();
        foreach (var doc in snap.Documents)
        {
            var d = doc.ToDictionary();

            list.Add(new ExpenseItem
            {
                Id = doc.Id,
                Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                Description = d.TryGetValue("description", out var ds) ? (string)ds : "",
                Amount = d.TryGetValue("amount", out var a) ? ToDouble(a) : 0d,

                OccurredAt = d.TryGetValue("occurredAt", out var t) && t is Timestamp ts
                                 ? ts.ToDateTime().ToLocalTime()
                                 : DateTime.Now
            });
        }
        return list;
    }

    private static double ToDouble(object v)
    {
        if (v is double d) return d;
        if (v is long l) return l;
        if (v is int i) return i;
        double.TryParse(v?.ToString(), out var x);
        return x;
    }
}