using Firebase.Auth;
using Leux.Services;

namespace Leux;

public partial class ProfilePage : ContentPage
{
    private readonly INavigationService _navigationService;
    private readonly FirebaseAuthClient _authClient;

    public ProfilePage(INavigationService navigationService, FirebaseAuthClient authClient)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _authClient = authClient;
        CurrencyPicker.SelectedIndex = 0;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profile", "Save (UI only)", "OK");
    }

    private async void OnChangePassword(object sender, EventArgs e)
    {
        await DisplayAlert("Profile", "Change Password (UI only)", "OK");
    }

    private async void OnSignOut(object sender, EventArgs e)
    {
        try
        {
            _authClient.SignOut();
            await _navigationService.NavigateToLoginAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to sign out: {ex.Message}", "OK");
        }
    }
}
