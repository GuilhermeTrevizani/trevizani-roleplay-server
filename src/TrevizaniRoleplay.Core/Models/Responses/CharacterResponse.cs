using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class CharacterResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CharacterSex Sex { get; set; }
    public string SexDescription { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime RegisterDate { get; set; }
    public string History { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? Staffer { get; set; }
    public bool CanResendApplication { get; set; }
    public bool CanApplyNamechange { get; set; }
    public string? FactionName { get; set; }
    public string? FactionRankName { get; set; }
    public string Job { get; set; } = string.Empty;
    public int ConnectedTime { get; set; }
    public uint Cellphone { get; set; }
    public int Bank { get; set; }
    public string Attributes { get; set; } = string.Empty;
    public IEnumerable<Vehicle> Vehicles { get; set; } = [];
    public IEnumerable<Property> Properties { get; set; } = [];
    public IEnumerable<Company> Companies { get; set; } = [];

    public class Vehicle
    {
        public Guid Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public IEnumerable<string> CharactersWithAccess { get; set; } = [];
    }

    public class Property
    {
        public Guid Id { get; set; }
        public uint Number { get; set; }
        public string Address { get; set; } = string.Empty;
        public IEnumerable<string> CharactersWithAccess { get; set; } = [];
    }

    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Safe { get; set; }
        public bool HasSafeAccess { get; set; }
    }
}