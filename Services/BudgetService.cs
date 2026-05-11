using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System.Diagnostics;

namespace Leux.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirestoreRestClient _rest;

        public BudgetService(FirebaseAuthClient auth, FirestoreRestClient rest)
        {
            _auth = auth;
            _rest = rest;
        }

        private async Task<string?> Token() =>
            _auth.User == null ? null : await _auth.User.GetIdTokenAsync(false);

        public async Task<bool> CreateBudgetAsync(string userId, Budget newBudget)
        {
            var token = await Token();
            if (token == null) return false;
            try
            {
                newBudget.BudgetId = Guid.NewGuid().ToString();
                newBudget.CreatedAt = Timestamp.GetCurrentTimestamp();
                var map = new Dictionary<string, object?>
                {
                    ["budgetId"] = newBudget.BudgetId,
                    ["name"] = newBudget.Name,
                    ["category"] = newBudget.Category ?? "",
                    ["spendingLimit"] = newBudget.SpendingLimit,
                    ["timePeriod"] = newBudget.TimePeriod ?? "",
                    ["startDate"] = newBudget.StartDate.ToDateTime(),
                    ["endDate"] = newBudget.EndDate.ToDateTime(),
                    ["createdAt"] = newBudget.CreatedAt.ToDateTime()
                };
                return await _rest.ArrayUnionAsync($"users/{userId}", "budgets", map, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving budget: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Budget>> GetUserBudgetsAsync(string userId)
        {
            var token = await Token();
            if (token == null) return new();
            try
            {
                var doc = await _rest.GetDocumentAsync($"users/{userId}", token);
                if (doc == null || !doc.TryGetValue("budgets", out var raw)) return new();
                var list = (List<object>)raw;
                return list.Select(b => ParseBudget((Dictionary<string, object>)b)).ToList();
            }
            catch { return new(); }
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
                Debug.WriteLine($"Error in BudgetService.GetUserExpensesAsync: {ex.Message}");
                return new();
            }
        }

        public async Task<List<BudgetSummary>> GetUserBudgetSummariesAsync(string userId)
        {
            var expenses = await GetUserExpensesAsync(userId);
            var budgets = await GetUserBudgetsAsync(userId);
            var summaries = new List<BudgetSummary>();
            foreach (var budget in budgets)
            {
                var inRange = expenses.Where(e => e.Date >= budget.StartDate && e.Date <= budget.EndDate);
                if (!string.IsNullOrEmpty(budget.Category))
                    inRange = inRange.Where(e => e.Category == budget.Category);
                summaries.Add(new BudgetSummary
                {
                    Name = budget.Name,
                    SpendingLimit = budget.SpendingLimit,
                    CurrentSpent = inRange.Sum(e => e.Cost)
                });
            }
            return summaries;
        }

        private static Budget ParseBudget(Dictionary<string, object> m)
        {
            static Timestamp ToTs(object v) =>
                Timestamp.FromDateTime(DateTime.SpecifyKind((DateTime)v, DateTimeKind.Utc));
            return new Budget
            {
                BudgetId = m.TryGetValue("budgetId", out var id) ? (string)id : "",
                Name = m.TryGetValue("name", out var n) ? (string)n : "",
                Category = m.TryGetValue("category", out var c) ? (string)c : "",
                SpendingLimit = m.TryGetValue("spendingLimit", out var sl) ? ToDouble(sl) : 0d,
                TimePeriod = m.TryGetValue("timePeriod", out var tp) ? (string)tp : "",
                StartDate = m.TryGetValue("startDate", out var sd) && sd is DateTime sdt ? ToTs(sdt) : default,
                EndDate = m.TryGetValue("endDate", out var ed) && ed is DateTime edt ? ToTs(edt) : default,
                CreatedAt = m.TryGetValue("createdAt", out var ca) && ca is DateTime cadt ? ToTs(cadt) : default,
            };
        }

        internal static ExpenseEntry ParseExpense(Dictionary<string, object> d)
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
