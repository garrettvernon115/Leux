namespace Leux;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        await DisplayAlert("Login Attempt",
            $"Email: {email}\nPassword: {password}", "OK");

        if (!IsEmailValid(email) || !IsValidRegister(email, password))
        {
            // email or password issue
        }

        // register user

    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistrationPage());
    }

    private bool IsEmailValid(string email)
    {
        return false;
    }

    private bool IsValidRegister(string email, string password)
    {
        return false;
    }
}
