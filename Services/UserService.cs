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
                return true;
            }
            catch (FirebaseAuthException)
            {
                return false;
            }
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
