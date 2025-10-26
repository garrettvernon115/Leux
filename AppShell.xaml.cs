namespace Leux;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
    }

    public void SwitchToMainApp()
    {
        Items.Clear();

        var tabBar = new TabBar
        {
            Route = "MainApp"
        };

        var dashboardTab = new Tab
        {
            Title = "Dashboard",
            Icon = "dashboard_icon.png",
            Route = "DashboardTab"
        };
        dashboardTab.Items.Add(new ShellContent
        {
            Route = "DashboardPage",
            ContentTemplate = new DataTemplate(typeof(DashboardPage))
        });
        tabBar.Items.Add(dashboardTab);

        var mainTab = new Tab
        {
            Title = "Home",
            Icon = "home_icon.png",
            Route = "MainTab"
        };
        mainTab.Items.Add(new ShellContent
        {
            Route = "MainPage",
            ContentTemplate = new DataTemplate(typeof(MainPage))
        });
        tabBar.Items.Add(mainTab);

        var profileTab = new Tab
        {
            Title = "Profile",
            Icon = "profile_icon.png",
            Route = "ProfileTab"
        };
        profileTab.Items.Add(new ShellContent
        {
            Route = "ProfilePage",
            ContentTemplate = new DataTemplate(typeof(ProfilePage))
        });
        tabBar.Items.Add(profileTab);

        Items.Add(tabBar);

        CurrentItem = tabBar;
    }

    public void SwitchToLogin()
    {
        Items.Clear();

        var loginContent = new ShellContent
        {
            Route = "Login",
            ContentTemplate = new DataTemplate(typeof(LoginPage))
        };

        Shell.SetNavBarIsVisible(loginContent, false);

        Items.Add(loginContent);
        CurrentItem = loginContent;
    }
}
