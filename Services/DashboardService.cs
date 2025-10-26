using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Leux.Resources.Models;


namespace Leux.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly FirestoreDb _firestoreDb;
        public DashboardService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
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
    }
}

