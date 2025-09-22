using Leux.Resources.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Firestore.Example
{
    public interface IFireStoreServiceCounter
    {
        Task<CounterData> GetCounterAsync();
        Task<bool> UpdateCounterAsync(int count);
        Task<bool> IncrementCounterAsync();
        Task<bool> ResetCounterAsync();
    }
}
