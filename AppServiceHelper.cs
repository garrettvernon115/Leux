namespace Leux;

public static class AppServiceHelper
{
    public static IServiceProvider Services
        => Application.Current?.Handler?.MauiContext?.Services
           ?? throw new InvalidOperationException("MAUI service provider not available.");
}
