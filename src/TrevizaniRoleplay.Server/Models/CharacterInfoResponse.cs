namespace TrevizaniRoleplay.Server.Models;

public class CharacterInfoResponse
{
    public Guid? FactionId { get; set; }
    public UserStaff Staff { get; set; }
    public DateTime RegisterDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CurrentDate { get; set; }
    public string User { get; set; } = string.Empty;
    public int PaycheckValue { get; set; }
    public int PaycheckMultiplier { get; set; }
    public IEnumerable<Paycheck> PaycheckItems { get; set; } = [];
    public IEnumerable<Item> Items { get; set; } = [];
    public IEnumerable<Property> Properties { get; set; } = [];
    public IEnumerable<Vehicle> Vehicles { get; set; } = [];
    public IEnumerable<Company> Companies { get; set; } = [];
    public IEnumerable<Invite> Invites { get; set; } = [];
    public string History { get; set; } = string.Empty;
    public string Premium { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string FactionName { get; set; } = string.Empty;
    public string RankName { get; set; } = string.Empty;
    public string Job { get; set; } = string.Empty;
    public int ConnectedTime { get; set; }
    public int NameChanges { get; set; }
    public int PlateChanges { get; set; }
    public int Bank { get; set; }
    public string Skin { get; set; } = string.Empty;
    public int Health { get; set; }
    public int Armor { get; set; }
    public string UsingDrug { get; set; } = string.Empty;
    public string EndDrugEffects { get; set; } = string.Empty;
    public string ThresoldDeath { get; set; } = string.Empty;
    public string ThresoldDeathReset { get; set; } = string.Empty;
    public int StaffDutyTime { get; set; }
    public int HelpRequestsAnswersQuantity { get; set; }
    public int PremiumPoints { get; set; }
    public int NumberChanges { get; set; }
    public int BankAccount { get; set; }

    public class Paycheck
    {
        public string Description { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool Debit { get; set; }
    }

    public class Item
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Extra { get; set; }
    }

    public class Property
    {
        public uint Number { get; set; }
        public string Address { get; set; } = string.Empty;
        public int Price { get; set; }
        public int ProtectionLevel { get; set; }
    }

    public class Company
    {
        public string Name { get; set; } = string.Empty;
        public bool Owner { get; set; }
    }

    public class Invite
    {
        public string Type { get; set; } = string.Empty;
        public string WaitingTime { get; set; } = string.Empty;
    }

    public class Vehicle
    {
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public int ProtectionLevel { get; set; }
        public bool XMR { get; set; }
    }
}