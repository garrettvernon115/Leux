using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Models
{
    public class ReportData
    {
        public string WeeklyTitle { get; set; }
        public double WeeklySpent { get; set; }
        public int WeeklyTx { get; set; }
        public double WeeklyAvg { get; set; }
        public string WeeklyTopCat { get; set; }
        public string BudgetUsed { get; set; }

        public string MonthlyTitle { get; set; }
        public double MonthlySpent { get; set; }
        public int MonthlyTx { get; set; }
        public double MonthlyAvg { get; set; }
        public string MonthlyBudgetUsed { get; set; }

        public List<CategoryData> Categories { get; set; }
    }

    public class CategoryData
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }
}
