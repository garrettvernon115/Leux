namespace Leux.Resources.Firestore
{
    // Firestore is now accessed via FirestoreRestClient using the authenticated user's token.
    public static class FirestoreDatabase
    {
        public static bool IsInitialized => true;
        public static Task<bool> InitializeAsync() => Task.FromResult(true);
    }
}
