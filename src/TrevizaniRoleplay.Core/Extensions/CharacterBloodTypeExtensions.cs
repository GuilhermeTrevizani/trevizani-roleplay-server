using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CharacterBloodTypeExtensions
{
    public static string GetDescription(this CharacterBloodType characterBloodType)
    {
        return characterBloodType switch
        {
            CharacterBloodType.APositive => Resources.APositive,
            CharacterBloodType.ANegative => Resources.ANegative,
            CharacterBloodType.BPositive => Resources.BPositive,
            CharacterBloodType.BNegative => Resources.BNegative,
            CharacterBloodType.ABPositive => Resources.ABPositive,
            CharacterBloodType.ABNegative => Resources.ABNegative,
            CharacterBloodType.OPositive => Resources.OPositive,
            CharacterBloodType.ONegative => Resources.ONegative,
            _ => characterBloodType.ToString(),
        };
    }
}