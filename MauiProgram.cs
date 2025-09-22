using Firebase.Auth;
using Firebase.Auth.Providers;
using Leux.Resources.Pages.Firestore.Example;
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
			Providers = new Firebase.Auth.Providers.FirebaseAuthProvider[]
			{
				new EmailProvider()
			}
        }));

		builder.Services.AddSingleton<IFireStoreServiceCounter, FirestoreServiceCounter>();

		return builder.Build();
	}
}
