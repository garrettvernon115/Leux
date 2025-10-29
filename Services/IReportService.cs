using Leux.Resources.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Services
{
    public interface IReportService
    {
        Task<ReportData> GetReportDataAsync(string userId);
    }
}