using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System.Diagnostics;

namespace Leux.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirestoreRestClient _rest;

        public DashboardService(FirebaseAuthClient auth, FirestoreRestClient rest)
        {
            _auth = auth;
            _rest = rest;
        }

        private async Task<string?> Token() =>
            _auth.User == null ? null : await _auth.User.GetIdTokenAsync(false);

        public async Task<bool> AddExpenseAsync(string userId, ExpenseEntry newExpense)
        {
            var token = await Token();
            if (token == null) return false;
            try
            {
                return await _rest.AddDocumentAsync($"users/{userId}/entries", new Dictionary<string, object?>
                {
                    ["description"] = newExpense.Name,
                    ["category"] = newExpense.Category,
                    ["amount"] = newExpense.Cost,
                    ["occurredAt"] = newExpense.Date.ToDateTime()
                }, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddExpenseAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ExpenseEntry>> GetUserExpensesAsync(string userId)
        {
            var token = await Token();
            if (token == null) return new();
            try
            {
                var docs = await _rest.ListDocumentsAsync($"users/{userId}/entries", token);
                return docs.Select(d => ParseExpense(d.Fields)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUserExpensesAsync: {ex.Message}");
                return new();
            }
        }

        private static ExpenseEntry ParseExpense(Dictionary<string, object> d)
        {
            static Timestamp ToTs(DateTime dt) =>
                Timestamp.FromDateTime(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
            return new ExpenseEntry
            {
                Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                Name = d.TryGetValue("description", out var ds) ? (string)ds : "",
                Cost = d.TryGetValue("amount", out var a) ? ToDouble(a) : 0d,
                Date = d.TryGetValue("occurredAt", out var t) && t is DateTime dt
                    ? ToTs(dt) : Timestamp.FromDateTime(DateTime.UtcNow)
            };
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
