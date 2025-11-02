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
        Routing.RegisterRoute(nameof(BudgetPage), typeof(BudgetPage));
    }

    public void SwitchToMainApp()
    {
        Items.Clear();

        // Create a TabBar for the main authenticated section
        var mainTabBar = new TabBar
        {
            Route = "MainApp"
        };

        // Budget Tab
        var budgetTab = new ShellContent
        {
            Title = "Budget",
            Route = "BudgetPage",
            ContentTemplate = new DataTemplate(typeof(BudgetPage))
        };
        Shell.SetNavBarIsVisible(budgetTab, false);

        // Dashboard Tab
        var dashboardTab = new ShellContent
        {
            Title = "Dashboard",
            Route = "DashboardPage",
            ContentTemplate = new DataTemplate(typeof(DashboardPage))
        };
        Shell.SetNavBarIsVisible(dashboardTab, false);

        // Expenses Tab
        /**
        var expensesTab = new ShellContent
        {
            Title = "Expenses",
            Route = "ExpensesPage",
            ContentTemplate = new DataTemplate(typeof(ExpensesPage))
        };
        Shell.SetNavBarIsVisible(expensesTab, false);
        **/

        // Profile Tab
        var profileTab = new ShellContent
        {
            Title = "Profile",
            Route = "ProfilePage",
            ContentTemplate = new DataTemplate(typeof(ProfilePage))
        };
        Shell.SetNavBarIsVisible(profileTab, false);

        // Reports Tab
        var reportsTab = new ShellContent
        {
            Title = "Reports",
            Route = "ReportPage",
            ContentTemplate = new DataTemplate(typeof(ReportPage))
        };
        Shell.SetNavBarIsVisible(reportsTab, false);

        // Add all tabs to the TabBar
        mainTabBar.Items.Add(budgetTab);
        mainTabBar.Items.Add(dashboardTab);
        // mainTabBar.Items.Add(expensesTab);
        mainTabBar.Items.Add(profileTab);
        mainTabBar.Items.Add(reportsTab);

        Items.Add(mainTabBar);
        CurrentItem = mainTabBar;

        // Navigate to Dashboard by default
        Shell.Current.GoToAsync("//MainApp/DashboardPage");
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
