using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Google.Cloud.Firestore;
using Leux.Resources.Firestore.Example;
using Leux.Resources.Models;
using Leux.Resources.Pages;
using Leux.Services;
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

        builder.Services.AddSingleton<FirestoreDb>(provider =>
        {
            try
            {
                var stream = FileSystem.OpenAppPackageFileAsync("leux-ed1c0-firebase-adminsdk-fbsvc-46b4a2c882.json")
                    .GetAwaiter().GetResult();

                string json;
                using (var reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }

                var tempPath = Path.Combine(FileSystem.CacheDirectory, "firebase-credentials.json");
                File.WriteAllText(tempPath, json);
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);

                return FirestoreDb.Create(projectId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize FirestoreDb: {ex.Message}", ex);
            }
        });

        builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
        {
            ApiKey = "AIzaSyA8dJzxokMX81gk5uU4P9bByaYNGFhGlC0",
            AuthDomain = "leux-ed1c0.firebaseapp.com",
            Providers = new[] { new EmailProvider() },
            UserRepository = new FileUserRepository("Leux")
        }));

        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IFireStoreServiceCounter, FirestoreServiceCounter>();
        builder.Services.AddSingleton<IDashboardService, DashboardService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IBudgetService, BudgetService>();

        // builder.Services.AddSingleton<IReportService, ReportService>(); TODO ReportService

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ReportPage>();
        builder.Services.AddTransient<BudgetPage>();

        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}