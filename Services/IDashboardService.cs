using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leux.Resources.Models;
using System.Threading.Tasks;

namespace Leux.Services
{
    public interface IDashboardService
    {
        Task<bool> AddExpenseAsync(string userId, ExpenseEntry newExpense);
    }
}

