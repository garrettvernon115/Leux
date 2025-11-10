using Google.Cloud.Firestore;

namespace Leux;

public class ExpenseItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = "Other";
    public string Description { get; set; } = "";
    public double Amount { get; set; }
    public Timestamp OccurredAt { get; set; }

    
    public string AmountText => $"${Amount:F2}";
    public string TimeText => OccurredAt.ToString();

    public Color CategoryColor => Category switch
    {
        "Food & Drink" => Color.FromArgb("#6A5ACD"),
        "Shopping" => Color.FromArgb("#4CAF50"),
        "Entertainment" => Color.FromArgb("#2196F3"),
        "Transport" => Color.FromArgb("#FF9800"),
        _ => Color.FromArgb("#9E9E9E"),
    };
}
