using System.Diagnostics;

namespace Leux;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
    }

    private async void RegisterUser(object sender, EventArgs e)
    {
        var email = emailEntered.Text;
        var password = passwordEntered.Text;
        var confirmPassword = confirmPasswordEntered.Text;

        if (!IsEmailValid(email) || !IsValidRegister(email, password, confirmPassword))
        {
            // email or password issue
        }

        // register user
    }

    private bool IsEmailValid(string email)
    {
        return false;
    }

    private bool IsValidRegister(string email, string password, string matchPassword)
    {
        return false;
    }

    private async void NavigateLoginPage(object sender , EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}