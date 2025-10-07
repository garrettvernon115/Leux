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

        
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Sign Up", "Navigate to Sign Up page (coming soon)", "OK");
    }
}
