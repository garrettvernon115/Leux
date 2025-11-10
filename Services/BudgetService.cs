using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System;
using System.Collections.Generic;
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

        //Implementation to get all budgets
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

        // Implementation to get all expenses
        public async Task<List<ExpenseEntry>> GetUserExpensesAsync(string userId)
        {
            try
            {
                DocumentReference userDocRef = _usersCollection.Document(userId);
                DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var userDoc = snapshot.ConvertTo<UserDocument>();
                    return userDoc?.Expenses ?? new List<ExpenseEntry>();
                }
                return new List<ExpenseEntry>();
            }
            catch
            {
                return new List<ExpenseEntry>();
            }
        }

        public async Task<List<BudgetSummary>> GetUserBudgetSummariesAsync(string userId)
        {
            // 1. Get all data in parallel
            Task<List<ExpenseEntry>> expensesTask = GetUserExpensesAsync(userId);
            Task<List<Budget>> budgetsTask = GetUserBudgetsAsync(userId);

            await Task.WhenAll(expensesTask, budgetsTask);

            List<ExpenseEntry> allExpenses = await expensesTask;
            List<Budget> allBudgets = await budgetsTask;

            var budgetSummaries = new List<BudgetSummary>();

            // 2. Process the data
            foreach (var budget in allBudgets)
            {
                // Filter expenses that fall within the budget's time window
                var expensesInTimeframe = allExpenses.Where(e =>
                    e.Date >= budget.StartDate && e.Date <= budget.EndDate
                );

                // If the budget has a category, filter by it.
                // If Category is null/empty, it's a "Monthly Budget" and we sum all categories.
                if (!string.IsNullOrEmpty(budget.Category))
                {
                    expensesInTimeframe = expensesInTimeframe.Where(e =>
                        e.Category == budget.Category
                    );
                }

                // 3. Sum the costs and create the summary object
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
    }
}