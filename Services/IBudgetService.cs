using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leux.Resources.Models;

namespace Leux.Services
{
    public interface IBudgetService
    {
        // Creating a new budget
        Task<bool> CreateBudgetAsync(string userId, Budget newBudget);

        // Loading all budgets to display
        Task<List<Budget>> GetUserBudgetsAsync(string userId);

        // Getting all expenses to perform calculations
        Task<List<ExpenseEntry>> GetUserExpensesAsync(string userId);

        Task<List<BudgetSummary>> GetUserBudgetSummariesAsync(string userId);
    }
}

