using Leux.Services;
using Leux.Resources.Models;
using Google.Cloud.Firestore;
using System.Diagnostics;
using Firebase.Auth;
namespace Leux
{
    public partial class DashboardPage : ContentPage
    {
        private readonly IDashboardService _dashboardService;
        private readonly FirebaseAuthClient _authClient;
        private readonly INavigationService _navigationService;

        public DashboardPage(IDashboardService dashboardService,FirebaseAuthClient authClient, INavigationService navigationService)
        {
            InitializeComponent();
            _dashboardService = dashboardService;
            _authClient = authClient;
            _navigationService = navigationService;
        }

        private async void OnAddExpenseClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the currently signed-in user's ID from the auth client.
                string currentUserId = _authClient?.User?.Uid;

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    await DisplayAlert("Not Logged In", "You must be logged in to add an expense.", "OK");
                    return;
                }

                var description = DescriptionEntry.Text;
                var amountText = AmountEntry.Text;
                var category = CategoryPicker.SelectedItem as string;

                if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(amountText) || string.IsNullOrWhiteSpace(category) || !double.TryParse(amountText, out double amount))
                {
                    await DisplayAlert("Error", "Please fill in all fields correctly.", "OK");
                    return;
                }

                var newExpense = new ExpenseEntry
                {
                    Name = description,
                    Category = category,
                    Cost = amount,
                    Date = Timestamp.GetCurrentTimestamp()
                };

                bool success = await _dashboardService.AddExpenseAsync(currentUserId, newExpense);

                if (success)
                {
                    await DisplayAlert("Success!", "A new expense was added to your record.", "OK");
                    DescriptionEntry.Text = "";
                    AmountEntry.Text = "";
                    CategoryPicker.SelectedIndex = -1;
                }
                else
                {
                    await DisplayAlert("Failure", "Could not add the expense.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
                Debug.WriteLine($"FULL ERROR: {ex}");
            }
        }
    }
}
