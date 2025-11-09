using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Models
{
    public class ReportData
    {
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
        public double WeeklySpent { get; set; }
        public int WeeklyTx { get; set; }
        public double WeeklyAvg { get; set; }
        public string WeeklyTopCat { get; set; }
        public string BudgetUsed { get; set; }

        public string MonthlyTitle
        {
            get
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                return $"Monthly Report ({monthStart:MMMM yyyy})";
            }
        }
        public double MonthlySpent { get; set; }
        public int MonthlyTx { get; set; }
        public double MonthlyAvg { get; set; }
        public string MonthlyBudgetUsed { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CategoryData> Categories { get; set; }
    }

    public class CategoryData
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
}
