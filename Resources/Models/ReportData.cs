using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace Leux.Resources.Models
{
    [FirestoreData]
    public class ReportData
    {
        [FirestoreProperty("weeklySpent")]
        public double WeeklySpent { get; set; }

        [FirestoreProperty("weeklyTx")]
        public int WeeklyTx { get; set; }

        [FirestoreProperty("weeklyAvg")]
        public double WeeklyAvg { get; set; }

        [FirestoreProperty("weeklyTopCat")]
        public string WeeklyTopCat { get; set; }

        [FirestoreProperty("budgetUsed")]
        public string BudgetUsed { get; set; }

        [FirestoreProperty("monthlySpent")]
        public double MonthlySpent { get; set; }

        [FirestoreProperty("monthlyTx")]
        public int MonthlyTx { get; set; }

        [FirestoreProperty("monthlyAvg")]
        public double MonthlyAvg { get; set; }

        [FirestoreProperty("monthlyBudgetUsed")]
        public string MonthlyBudgetUsed { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("categories")]
        public List<CategoryData> Categories { get; set; }

        // Computed properties (no Firestore attribute)
        public string WeeklyTitle
        {
            get
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = today.AddDays(-diff);
                var weekEnd = weekStart.AddDays(6);
                return $"Weekly Report ({weekStart:MMM dd} - {weekEnd:MMM d, yyyy})";
            }
        }

        public string MonthlyTitle
        {
            get
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                return $"Monthly Report ({monthStart:MMMM yyyy})";
            }
        }
    }

    [FirestoreData]
    public class CategoryData
    {
        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("amount")]
        public double Amount { get; set; }
    }
}