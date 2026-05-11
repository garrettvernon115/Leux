using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Grpc.Core;
using System.Diagnostics;

namespace Leux.Resources.Firestore
{
    public static class FirestoreDatabase
    {
        private static FirestoreDb _db;
        private static bool _isInitialized = false;
        private static readonly string _projectID = "leux-new";

        public static FirestoreDb Database => _db;
        public static bool IsInitialized => _isInitialized;

        public static async Task<bool> InitializeAsync()
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("leux-new-firebase-adminsdk.json");
                string json;
                using (var reader = new StreamReader(stream))
                    json = await reader.ReadToEndAsync();

                GoogleCredential credential = GoogleCredential.FromJson(json);
                ChannelCredentials channelCredentials = credential.ToChannelCredentials();

                FirestoreDbBuilder builder = new FirestoreDbBuilder
                {
                    ProjectId = _projectID,
                    ChannelCredentials = channelCredentials
                };

                _db = builder.Build();
                _isInitialized = true;
                Debug.WriteLine($"Firestore initialized");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firestore initialization error: {ex.Message}");
                _isInitialized = false;
                throw;
            }
        }
    }
}
