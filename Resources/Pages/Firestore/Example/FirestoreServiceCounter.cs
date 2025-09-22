using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Grpc.Core;
using Leux.Resources.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leux.Resources.Pages.Firestore.Example
{
    public class FirestoreServiceCounter : IFireStoreServiceCounter
    {
        private FirestoreDb _db;
        private const string CollectionName = "app_data";
        private const string DocumentID = "counter";
        private const string ProjectId = "leux-ed1c0";
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Credentials file not found at: {path}");
                }

                Debug.WriteLine("Credentials file found!");
                GoogleCredential credential = GoogleCredential.FromFile(path);
                ChannelCredentials channelCredentials = credential.ToChannelCredentials();

                FirestoreDbBuilder builder = new FirestoreDbBuilder
                {
                    ProjectId = ProjectId,
                    ChannelCredentials = channelCredentials
                };

                _db = builder.Build();
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firestore initialization error: {ex.Message}");
                _isInitialized = false;
                throw;
            }
        }

        public async Task<CounterData> GetCounterAsync()
        {
            try
            {
                if (!_isInitialized)
                {
                    Debug.WriteLine("Firestore not initialized");
                    return new CounterData();
                }

                DocumentReference docRef = _db.Collection(CollectionName).Document(DocumentID);
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
                if (!_isInitialized)
                {
                    Debug.WriteLine("Firestore not initialized");
                    return false;
                }

                DocumentReference docRef = _db.Collection(CollectionName).Document(DocumentID);

                await _db.RunTransactionAsync(async transaction =>
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
                if (!_isInitialized && !await InitializeAsync())
                {
                    Debug.WriteLine("Firestore not initialized");
                    return false;
                }

                DocumentReference docRef = _db.Collection(CollectionName).Document(DocumentID);

                await _db.RunTransactionAsync(async transaction =>
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
