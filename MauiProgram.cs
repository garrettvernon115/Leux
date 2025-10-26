using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;
using Leux.Services;
using Leux.Resources.Firestore.Example;
using Microsoft.Extensions.Logging;

namespace Leux;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
        {
            ApiKey = "AIzaSyA8dJzxokMX81gk5uU4P9bByaYNGFhGlC0",
            AuthDomain = "leux-ed1c0.firebaseapp.com",
            Providers = new[] { new EmailProvider() }
        }));


        string projectId = "leux-ed1c0";
        builder.Services.AddSingleton(provider =>
        {
            var stream = FileSystem.OpenAppPackageFileAsync("leux-ed1c0-firebase-adminsdk-fbsvc-46b4a2c882.json").Result;
            string json;
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", json);
            return FirestoreDb.Create(projectId);
        });
        builder.Services.AddSingleton<IUserService, UserService>();

        builder.Services.AddSingleton<IFireStoreServiceCounter, FirestoreServiceCounter>();
        builder.Services.AddSingleton<IDashboardService, DashboardService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<RegistrationPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}

