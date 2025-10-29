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
        string newPassword = await DisplayPromptAsync("Change Password", "Enter your new password:", keyboard: Keyboard.Default, initialValue: "", maxLength: -1, placeholder: "New Password", accept: "OK", cancel: "Cancel");

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return; 
        }

        if (newPassword.Length < 6)
        {
            await DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
            return;
        }

        try
        {           
            if (_userService == null)
            {
                await DisplayAlert("Error", "User service is not available. Cannot change password.", "OK");
                return;
            }

            bool success = await _userService.ChangePasswordAsync(newPassword);

            if (success)
            {
                await DisplayAlert("Success", "Your password has been changed successfully.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Could not change password. Please try logging out and back in.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Change Password Error: {ex}");
        }
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
