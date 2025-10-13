using Leux.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Leux;

public partial class RegistrationPage : ContentPage
{
    private readonly IUserService _userService;

    public RegistrationPage()
    {
        InitializeComponent();

        _userService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<IUserService>();
    }

    private async void RegisterUser(object sender, EventArgs e)
    {
        var name = nameEntered.Text;
        var email = emailEntered.Text;
        var password = passwordEntered.Text;
        var confirmPassword = confirmPasswordEntered.Text;

        if (!IsValidRegister(name, email, password, confirmPassword))
        {
            return;
        }

        bool success = await _userService.RegisterUserAsync(name, email, password);

        if (success)
        {
            await DisplayAlert("Success", "Your account has been created!", "OK");
            await Navigation.PushAsync(new LoginPage());
        }
        else
        {
            statusMessage.Text = "Registration failed. (This is a mock failure).";
        }
    }

    private bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsValidRegister(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            statusMessage.Text = "Please fill in all fields.";
            return false;
        }

        if (!IsEmailValid(email))
        {
            statusMessage.Text = "Please enter a valid email address.";
            return false;
        }

        if (password.Length < 6)
        {
            statusMessage.Text = "Password must be at least 6 characters long.";
            return false;
        }

        if (password != confirmPassword)
        {
            statusMessage.Text = "Passwords do not match.";
            return false;
        }

        statusMessage.Text = ""; 
        return true;
    }

    private async void NavigateLoginPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}

