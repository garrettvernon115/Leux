using Leux.Resources.Firestore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Leux;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
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
    }

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}