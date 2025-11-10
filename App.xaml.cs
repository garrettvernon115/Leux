using Leux.Resources.Firestore;
using Leux.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Leux;

public partial class App : Application
{

	private readonly IUserService _userService;

	public App(IUserService userService)
	{
		InitializeComponent();
		_userService = userService;
	}

    protected override async void OnStart()
    {
        base.OnStart();
		
		try
		{
			await FirestoreDatabase.InitializeAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Firestore failed to connect!");
		}

        // await Shell.Current.GoToAsync($"{nameof(SplashPage)}");

        await Task.Delay(100); // TODO make longer for splash screen


		try
		{
			bool isLoggedIn = await _userService.IsUserLoggedInAsync();

			if (isLoggedIn)
			{
                await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
            }
			else
			{
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
		}
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation failed: {ex.Message}");
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
    }

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}