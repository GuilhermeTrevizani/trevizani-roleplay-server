using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class MyFactionDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasDuty { get; set; }
    public IEnumerable<FactionFlag> UserFlags { get; set; } = [];
    public string Color { get; set; } = string.Empty;
    public string ChatColor { get; set; } = string.Empty;
    public bool IsLeader { get; set; }
    public IEnumerable<SelectOptionResponse> FlagsOptions { get; set; } = [];
    public IEnumerable<Character> Characters { get; set; } = [];
    public IEnumerable<Vehicle> Vehicles { get; set; } = [];
    public IEnumerable<Rank> Ranks { get; set; } = [];

    public class Character
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid RankId { get; set; }
        public string RankName { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime LastAccessDate { get; set; }
        public int Position { get; set; }
        public bool IsOnline { get; set; }
        public double AverageMinutesOnDutyLastTwoWeeks { get; set; }
        public string FlagsJson { get; set; } = string.Empty;
        public IEnumerable<string> Flags { get; set; } = [];
    }

    public class Rank
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Salary { get; set; }
    }

    public class Vehicle
    {
        public Guid Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}