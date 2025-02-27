namespace TrevizaniRoleplay.Core.Models.Responses;

public class GetPremiumResponse
{
    public string? CurrentPurchaseName { get; set; }
    public string? CurrentPurchasePreferenceId { get; set; }
    public IEnumerable<Package> Packages { get; set; } = [];
    public IEnumerable<Item> Items { get; set; } = [];

    public class Package
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int OriginalValue { get; set; }
        public int Value { get; set; }
    }

    public class Item
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}