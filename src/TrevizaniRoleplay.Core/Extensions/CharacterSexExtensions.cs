using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class CharacterSexExtensions
{
    public static string GetDescription(this CharacterSex characterSex)
    {
        return characterSex switch
        {
            CharacterSex.Woman => Globalization.Resources.Woman,
            CharacterSex.Man => Globalization.Resources.Man,
            _ => characterSex.ToString()
        };
    }
}