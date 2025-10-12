using Google.Cloud.Firestore;

namespace Leux.Resources.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty("user_id")]
        public string UserId { get; set; }

        [FirestoreProperty("email")]
        public string Email { get; set; }

        [FirestoreProperty("password")]
        public string Password { get; set; }

        [FirestoreProperty("username")]
        public string Username { get; set; }
    }
}
