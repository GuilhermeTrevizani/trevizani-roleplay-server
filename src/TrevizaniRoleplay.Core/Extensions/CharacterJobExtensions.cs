using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CharacterJobExtensions
{
    public static string GetDescription(this CharacterJob characterJob)
    {
        return characterJob switch
        {
            CharacterJob.Unemployed => Globalization.Resources.Unemployed,
            CharacterJob.TaxiDriver => Globalization.Resources.TaxiDriver,
            CharacterJob.Mechanic => Globalization.Resources.Mechanic,
            CharacterJob.GarbageCollector => Globalization.Resources.GarbageCollector,
            CharacterJob.Trucker => Globalization.Resources.Trucker,
            _ => characterJob.ToString()
        };
    }
}