using Leux.Services;

namespace Leux;

public partial class ProfilePage : ContentPage
{
    private readonly IUserService _userService;

    public ProfilePage()
    {
        InitializeComponent();
        CurrencyPicker.SelectedIndex = 0;

        _userService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<IUserService>();
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

    private void OnSignOut(object sender, EventArgs e)
    {
        Application.Current.MainPage = new LoginPage();
    }
}
