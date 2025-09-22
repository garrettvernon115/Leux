using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Grpc.Core;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Firestore.Example
{
    public class FirestoreServiceCounter : IFireStoreServiceCounter
    {
        
        private const string CollectionName = "app_data";
        private const string DocumentID = "counter";
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public async Task<CounterData> GetCounterAsync()
        {
            try
            {
                if (!FirestoreDatabase.IsInitialized)
                {
                    Debug.WriteLine("Firestore not initialized");
                    return new CounterData();
                }

                DocumentReference docRef = FirestoreDatabase.Database.Collection(CollectionName).Document(DocumentID);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    CounterData counterData = snapshot.ConvertTo<CounterData>();
                    return counterData ?? new CounterData();
                }
                else
                {
                    var initialData = new CounterData();
                    await docRef.SetAsync(initialData);
                    return initialData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting counter: {ex.Message}");
                return new CounterData();
            }
        }

        public async Task<bool> UpdateCounterAsync(int count)
        {
            try
            {
                if (!FirestoreDatabase.IsInitialized)
                {
                    Debug.WriteLine("Firestore not initialized");
                    return false;
                }

                DocumentReference docRef = FirestoreDatabase.Database.Collection(CollectionName).Document(DocumentID);

                await FirestoreDatabase.Database.RunTransactionAsync(async transaction =>
                {
                    DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);
                    int currentCount = 0;
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<CounterData>();
                        currentCount = data?.Count ?? 0;
                    }

                    var updateData = new CounterData(currentCount + 1);
                    transaction.Set(docRef, updateData);
                });

                Debug.WriteLine("Counter incremented successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting counter: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IncrementCounterAsync()
        {
            try
            {
                if (!FirestoreDatabase.IsInitialized)
                {
                    Debug.WriteLine("Firestore not initialized");
                    return false;
                }

                DocumentReference docRef = FirestoreDatabase.Database.Collection(CollectionName).Document(DocumentID);

                await FirestoreDatabase.Database.RunTransactionAsync(async transaction =>
                {
                    DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);

                    int currentCount = 0;
                    if (snapshot.Exists)
                    {
                        var data = snapshot.ConvertTo<CounterData>();
                        currentCount = data?.Count ?? 0;
                    }

                    var updatedData = new CounterData(currentCount + 1);
                    transaction.Set(docRef, updatedData);
                });

                Debug.WriteLine("Counter incremented successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error incrementing counter: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetCounterAsync()
        {
            return await UpdateCounterAsync(0);
        }
    }
}
