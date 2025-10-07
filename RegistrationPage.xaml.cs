using System.Diagnostics;

namespace Leux;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("CLICKED");
    }
}