using Leux.Resources.Firestore.Example;
using Leux.Resources.Models;

namespace Leux;

public partial class MainPage : ContentPage
{
    private CounterData countData = new CounterData();
    private readonly IFireStoreServiceCounter _firestoreServiceCounter;

    public MainPage()
    {
        InitializeComponent();
        countData = new CounterData();
        _firestoreServiceCounter = Application.Current.MainPage.Handler.MauiContext.Services.GetService<IFireStoreServiceCounter>();
        LoadCounterAsync();
    }

    private async void LoadCounterAsync()
    {
        try
        {
            CounterBtn.IsEnabled = false;
            CounterBtn.Text = "Loading...";

            countData = await _firestoreServiceCounter.GetCounterAsync();

            UpdateButtonText();
            CounterBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load counter: {ex.Message}", "OK");
            CounterBtn.Text = "Click me";
            CounterBtn.IsEnabled = true;
        }
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        try
        {
            CounterBtn.IsEnabled = false;
            countData.Count++;
            UpdateButtonText();

            bool success = await _firestoreServiceCounter.IncrementCounterAsync();

            if (!success)
            {
                countData.Count--;
                UpdateButtonText();
                await DisplayAlert("Error", "Failed to save counter", "OK");
            }
            else
            {
                countData = await _firestoreServiceCounter.GetCounterAsync();
                UpdateButtonText();
            }
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        catch (Exception ex)
        {
            countData.Count--;
            UpdateButtonText();
            await DisplayAlert("Error", $"Failed to save counter: {ex.Message}", "OK");
        }
        finally
        {
            CounterBtn.IsEnabled = true;
        }
    }

    private void UpdateButtonText()
    {
        if (countData.Count == 0)
            CounterBtn.Text = "Click me";
        else if (countData.Count == 1)
            CounterBtn.Text = $"Clicked {countData.Count} time";
        else
            CounterBtn.Text = $"Clicked {countData.Count} times from everyone!";
    }
}
