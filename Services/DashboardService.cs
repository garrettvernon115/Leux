using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Leux.Resources.Models;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Firebase.Auth;



namespace Leux.Services
{
    public class DashboardService : IDashboardService
    {
        
        private readonly FirestoreDb _firestoreDb = FirestoreDatabase.Database;
        private readonly CollectionReference _usersCollection;

        public DashboardService()
        {
            _usersCollection = _firestoreDb.Collection("users");
        }

        public async Task<bool> AddExpenseAsync(string userId, ExpenseEntry newExpense)
        {
            try
            {
                DocumentReference userDocRef = _firestoreDb.Collection("users").Document(userId);
                await userDocRef.UpdateAsync("expenses", FieldValue.ArrayUnion(newExpense));
                return true;
            }
            catch
            {
                return false;
            }
        }

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
    }
}

