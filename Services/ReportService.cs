using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Leux.Resources.Models;

namespace Leux.Services
{
    public class ReportService : IReportService
    {
        private readonly FirestoreDb _firestoreDb;
        public ReportService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // public async Task<ReportData> GetReportDataAsync(string userId) { }
    }
}
