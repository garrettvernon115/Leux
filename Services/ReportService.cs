using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;

namespace Leux.Services
{
    public class ReportService : IReportService
    {
        private readonly CollectionReference _usersCollection;
        public ReportService()
        {
            _usersCollection = FirestoreDatabase.Database.Collection("users");
        }

        public async Task<ReportData?> GetReportDataAsync(string userId)
        {
            try
            {
                DocumentReference userDocRef = _usersCollection.Document(userId);
                DocumentSnapshot userSnapshot = await userDocRef.GetSnapshotAsync();
                if (userSnapshot.Exists)
                {
                    var userDoc = userSnapshot.ConvertTo<UserDocument>();
                    return userDoc.Reports?.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
