using Leux.Resources.Firestore;
using Leux.Services;
using System.Text.RegularExpressions;

namespace Leux;

public partial class LoginPage : ContentPage
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;

    public LoginPage(IUserService userService, INavigationService navigationService)
    {
        InitializeComponent();
        _userService = userService;
        _navigationService = navigationService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        // Validate input
        if (!IsValidLogin(email, password))
        {
            return;
        }

        // Attempt login
        bool success = await _userService.LoginUserAsync(email, password);

        if (success)
        {
            await DisplayAlert("Success", "Login successful!", "OK");
            await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid email or password. Please try again.", "OK");
        }
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await _navigationService.NavigateToRegistrationAsync();
    }

    private bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsValidLogin(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            DisplayAlert("Validation Error", "Please fill in all fields.", "OK");
            return false;
        }

        if (!IsEmailValid(email))
        {
            DisplayAlert("Validation Error", "Please enter a valid email address.", "OK");
            return false;
        }

        if (password.Length < 6)
        {
            DisplayAlert("Validation Error", "Password must be at least 6 characters long.", "OK");
            return false;
        }

        return true;
    }
}