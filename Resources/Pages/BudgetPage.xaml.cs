using System.Diagnostics;

namespace Leux;

public partial class BudgetPage : ContentPage
{
    public BudgetPage()
    {
        InitializeComponent();
    }

    private void OnAddBudgetClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private void OnSaveBudgetClicked(object sender, EventArgs e)
    {
        string name = BudgetNameEntry.Text;
        string amountText = BudgetAmountEntry.Text;
        DateTime date = BudgetDatePicker.Date;

        // TODO: Validate and save budget data

        PopupOverlay.IsVisible = false;
    }
}