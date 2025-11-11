using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics; 
using System.Linq;
using System.Threading.Tasks;

namespace Leux.Services
{
    [FirestoreData]
    internal class UserDocument
    {
        
        [FirestoreProperty("expenses")]
        public List<ExpenseEntry> Expenses { get; set; }

        [FirestoreProperty("budgets")]
        public List<Budget> Budgets { get; set; }

        [FirestoreProperty("reports")]
        public List<ReportData> Reports { get; set; }
    }

    public class BudgetService : IBudgetService
    {
        private readonly FirestoreDb _firestoreDb = FirestoreDatabase.Database;
        private readonly CollectionReference _usersCollection;

        public BudgetService()
        {
            _usersCollection = _firestoreDb.Collection("users");
        }

        public async Task<bool> CreateBudgetAsync(string userId, Budget newBudget)
        {
            try
            {
                newBudget.BudgetId = Guid.NewGuid().ToString();
                newBudget.CreatedAt = Timestamp.GetCurrentTimestamp();

                DocumentReference userDocRef = _usersCollection.Document(userId);
                await userDocRef.UpdateAsync("budgets", FieldValue.ArrayUnion(newBudget));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving budget: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Budget>> GetUserBudgetsAsync(string userId)
        {
            try
            {
                DocumentReference userDocRef = _usersCollection.Document(userId);
                DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var userDoc = snapshot.ConvertTo<UserDocument>();
                    return userDoc?.Budgets ?? new List<Budget>();
                }
                return new List<Budget>();
            }
            catch
            {
                return new List<Budget>();
            }
        }

        
        public async Task<List<ExpenseEntry>> GetUserExpensesAsync(string userId)
        {
            var expenses = new List<ExpenseEntry>();
            try
            {
               
                CollectionReference entriesCol = _usersCollection.Document(userId).Collection("entries");
                QuerySnapshot snapshot = await entriesCol.GetSnapshotAsync();

                
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    var d = doc.ToDictionary();
                    var entry = new ExpenseEntry
                    {
                        
                        Name = d.TryGetValue("description", out var ds) ? (string)ds : "",
                        Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                        Cost = ToDouble(d.TryGetValue("amount", out var a) ? a : 0d),
                        Date = d.TryGetValue("occurredAt", out var t) && t is Timestamp ts ? ts : Timestamp.FromDateTime(DateTime.UtcNow)
                    };
                    expenses.Add(entry);
                }
                return expenses;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BudgetService.GetUserExpensesAsync: {ex.Message}");
                return expenses; 
            }
        }

        public async Task<List<BudgetSummary>> GetUserBudgetSummariesAsync(string userId)
        {
            Task<List<ExpenseEntry>> expensesTask = GetUserExpensesAsync(userId);
            Task<List<Budget>> budgetsTask = GetUserBudgetsAsync(userId);

            await Task.WhenAll(expensesTask, budgetsTask);

            List<ExpenseEntry> allExpenses = await expensesTask;
            List<Budget> allBudgets = await budgetsTask;

            var budgetSummaries = new List<BudgetSummary>();

            
            foreach (var budget in allBudgets)
            {
               
                var expensesInTimeframe = allExpenses.Where(e =>
                    e.Date >= budget.StartDate && e.Date <= budget.EndDate
                );

             
                if (!string.IsNullOrEmpty(budget.Category))
                {
                    expensesInTimeframe = expensesInTimeframe.Where(e =>
                        e.Category == budget.Category
                    );
                }

                
                double currentSpent = expensesInTimeframe.Sum(e => e.Cost);

                budgetSummaries.Add(new BudgetSummary
                {
                    Name = budget.Name,
                    SpendingLimit = budget.SpendingLimit,
                    CurrentSpent = currentSpent
                });
            }

            return budgetSummaries;
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
