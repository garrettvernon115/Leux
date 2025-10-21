using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Leux.Resources.Models
{
    [FirestoreData]
    public class ExpenseEntry
    {
        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("category")]
        public string Category { get; set; }

        [FirestoreProperty("cost")]
        public double Cost { get; set; }

        [FirestoreProperty("date")]
        public Timestamp Date { get; set; }
    }
}

