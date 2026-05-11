using Firebase.Auth;
using Leux.Resources.Firestore;
using Leux.Resources.Models;

namespace Leux.Services
{
    public class ReportService : IReportService
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirestoreRestClient _rest;

        public ReportService(FirebaseAuthClient auth, FirestoreRestClient rest)
        {
            _auth = auth;
            _rest = rest;
        }

        private async Task<string?> Token() =>
            _auth.User == null ? null : await _auth.User.GetIdTokenAsync(false);

        public async Task<ReportData?> GetReportDataAsync(string userId)
        {
            var token = await Token();
            if (token == null) return null;
            try
            {
                var doc = await _rest.GetDocumentAsync($"users/{userId}", token);
                if (doc == null || !doc.TryGetValue("reports", out var raw)) return null;
                var list = (List<object>)raw;
                return list
                    .Select(r => ParseReport((Dictionary<string, object>)r))
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();
            }
            catch { return null; }
        }

        private static ReportData ParseReport(Dictionary<string, object> m)
        {
            return new ReportData
            {
                WeeklySpent = ToDouble(m.TryGetValue("weeklySpent", out var ws) ? ws : 0d),
                WeeklyTx = (int)ToLong(m.TryGetValue("weeklyTx", out var wt) ? wt : 0L),
                WeeklyAvg = ToDouble(m.TryGetValue("weeklyAvg", out var wa) ? wa : 0d),
                WeeklyTopCat = m.TryGetValue("weeklyTopCat", out var wtc) ? (string)wtc : "",
                BudgetUsed = m.TryGetValue("budgetUsed", out var bu) ? (string)bu : "",
                MonthlySpent = ToDouble(m.TryGetValue("monthlySpent", out var ms) ? ms : 0d),
                MonthlyTx = (int)ToLong(m.TryGetValue("monthlyTx", out var mt) ? mt : 0L),
                MonthlyAvg = ToDouble(m.TryGetValue("monthlyAvg", out var ma) ? ma : 0d),
                MonthlyBudgetUsed = m.TryGetValue("monthlyBudgetUsed", out var mbu) ? (string)mbu : "",
                CreatedAt = m.TryGetValue("createdAt", out var ca) && ca is DateTime dt ? dt : DateTime.UtcNow,
            };
        }

        private static double ToDouble(object v)
        {
            if (v is double d) return d;
            if (v is long l) return l;
            if (v is int i) return i;
            return double.TryParse(v?.ToString(), out var x) ? x : 0d;
        }

        private static long ToLong(object v)
        {
            if (v is long l) return l;
            if (v is int i) return i;
            if (v is double d) return (long)d;
            return long.TryParse(v?.ToString(), out var x) ? x : 0L;
        }
    }
}
