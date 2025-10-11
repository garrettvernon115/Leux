namespace Leux;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
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

    private void OnSignOut(object sender, EventArgs e)
    {
        Application.Current.MainPage = new LoginPage();
    }
}
