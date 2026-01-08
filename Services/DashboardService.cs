using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Leux.Resources.Models;
<<<<<<< Updated upstream
using Leux.Resources.Firestore;
using System.Diagnostics;
=======

>>>>>>> Stashed changes

namespace Leux.Services
{
    public class DashboardService : IDashboardService
    {
<<<<<<< Updated upstream
        private readonly FirestoreDb _firestoreDb = FirestoreDatabase.Database;
        private readonly CollectionReference _usersCollection;

        public DashboardService()
        {
            _usersCollection = _firestoreDb.Collection("users");
=======
        private readonly FirestoreDb _firestoreDb;
        public DashboardService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
>>>>>>> Stashed changes
        }

        public async Task<bool> AddExpenseAsync(string userId, ExpenseEntry newExpense)
        {
            try
            {
<<<<<<< Updated upstream
                CollectionReference entriesCol = _usersCollection.Document(userId).Collection("entries");

               
                var expenseData = new Dictionary<string, object>
                {
                    { "description", newExpense.Name },
                    { "category", newExpense.Category },
                    { "amount", newExpense.Cost },
                    { "occurredAt", newExpense.Date } 
                };

               
                await entriesCol.AddAsync(expenseData);
=======
                DocumentReference userDocRef = _firestoreDb.Collection("users").Document(userId);
                await userDocRef.UpdateAsync("expenses", FieldValue.ArrayUnion(newExpense));
>>>>>>> Stashed changes
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddExpenseAsync: {ex.Message}");
                return false;
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
                        
                        Category = d.TryGetValue("category", out var c) ? (string)c : "Other",
                        Name = d.TryGetValue("description", out var ds) ? (string)ds : "",
                        Cost = ToDouble(d.TryGetValue("amount", out var a) ? a : 0d),
                        Date = d.TryGetValue("occurredAt", out var t) && t is Timestamp ts ? ts : Timestamp.FromDateTime(DateTime.UtcNow)
                    };
                    expenses.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUserExpensesAsync: {ex.Message}");
            }
            return expenses;
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