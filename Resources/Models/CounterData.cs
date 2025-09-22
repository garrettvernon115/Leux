using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Models
{
    [FirestoreData]
    public class CounterData
    {
        [FirestoreProperty("count")]
        public int Count { get; set; }

        [FirestoreProperty("lastUpdated")]
        public Timestamp LastUpdated { get; set; }

        public CounterData()
        {
            Count = 0;
            LastUpdated = Timestamp.GetCurrentTimestamp();
        }

        public CounterData(int count)
        {
            Count = count;
            LastUpdated = Timestamp.GetCurrentTimestamp();
        }
    }
}
