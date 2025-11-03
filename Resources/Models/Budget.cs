using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Leux.Resources.Models
{
    [FirestoreData]
    public class Budget
    {
        [FirestoreProperty("budgetId")]
        public string BudgetId { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        // Leave this null or empty for a "Monthly Budget" that tracks all spending.
        [FirestoreProperty("category")]
        public string Category { get; set; }

        [FirestoreProperty("spendingLimit")]
        public double SpendingLimit { get; set; }

        [FirestoreProperty("timePeriod")]
        public string TimePeriod { get; set; }

        [FirestoreProperty("startDate")]
        public Timestamp StartDate { get; set; }

        [FirestoreProperty("endDate")]
        public Timestamp EndDate { get; set; }

        [FirestoreProperty("createdAt")]
        public Timestamp CreatedAt { get; set; }
    }
}