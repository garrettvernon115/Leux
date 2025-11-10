
using Microsoft.Maui.Controls;
using Leux.Resources.Firestore.Example;
using Leux.Resources.Models;

namespace Leux.Resources.Pages;

public partial class NavigationHeader : ContentView
{
    public NavigationHeader()
    {
        InitializeComponent();
    }

    private async void OnBudgetClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(BudgetPage)}");
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
    }

    private async void OnExpensesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ExpensesPage)}");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ProfilePage)}");
    }

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ReportPage)}");
    }
}