using Firebase.Auth;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore;
using Leux.Resources.Models;
using System.Threading.Tasks;

namespace Leux.Services
{
    public class UserService : IUserService
    {
        private readonly FirebaseAuthClient _authClient;
        private const string AUTH_TOKEN_KEY = "auth_token";
        private const string USER_ID_KEY = "user_id";

        public FirebaseAuthClient AuthClient => _authClient;

        public UserService(FirebaseAuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password)
        {
            try
            {
                var authCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
                string userId = authCredential.User.Uid;

                var user = new Leux.Resources.Models.User
                {
                    UserId = userId,
                    Email = email,
                    Username = username
                };

                CollectionReference usersCollection = FirestoreDatabase.Database.Collection("users");
                await usersCollection.Document(userId).SetAsync(user);
                return true;
            }
            catch (FirebaseAuthException)
            {
                return false;
            }
        }

        public async Task<bool> LoginUserAsync(string email, string password)
        {
            try
            {
                await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                if (_authClient?.User != null)
                {
                    string token = await _authClient.User.GetIdTokenAsync(false);
                    await SecureStorage.SetAsync(AUTH_TOKEN_KEY, token);
                    await SecureStorage.SetAsync(USER_ID_KEY, _authClient.User.Uid);
                }
                return true;
            }
            catch (FirebaseAuthException)
            {
                return false;
            }
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            try
            {
                string token = await SecureStorage.GetAsync(AUTH_TOKEN_KEY);
                System.Diagnostics.Debug.WriteLine($"Token exists: {!string.IsNullOrEmpty(token)}");
                System.Diagnostics.Debug.WriteLine($"_authClient.User is null: {_authClient.User == null}");

                if (string.IsNullOrEmpty(token))
                    return false;

                return true; // If token exists, trust it
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsUserLoggedInAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _authClient.SignOut();
            SecureStorage.Remove(AUTH_TOKEN_KEY);
            SecureStorage.Remove(USER_ID_KEY);
        }

        // New Implementation for changing the password
        public async Task<bool> ChangePasswordAsync(string newPassword)
        {         
            if (_authClient.User == null)
            {
                return false;
            }

            try
            {
                await _authClient.User.ChangePasswordAsync(newPassword);
                return true;
            }
            catch (FirebaseAuthException)
            {
                return false;
            }
        }
    }
}
