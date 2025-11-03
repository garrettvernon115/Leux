using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Models
{

    public class BudgetSummary
    {
        public string Name { get; set; }
        public double SpendingLimit { get; set; }
        public double CurrentSpent { get; set; }

        public double AmountRemaining => SpendingLimit - CurrentSpent;

        public double PercentSpent => (SpendingLimit > 0) ? (CurrentSpent / SpendingLimit) : 0;
    }
}