namespace TrevizaniRoleplay.Server.Extensions;

public static class FactionUnitExtension
{
    public static string GetCharacters(this FactionUnit factionUnit)
    {
        var characters = factionUnit.Characters!.Select(x => x.Character!.Name).Append(factionUnit.Character!.Name);
        return string.Join(", ", characters);
    }
}