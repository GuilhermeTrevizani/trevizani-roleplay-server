using TrevizaniRoleplay.Core.Globalization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class ForensicTestItemTypeExtensions
{
    public static string GetDescription(this ForensicTestItemType forensicTestItemType)
    {
        return forensicTestItemType switch
        {
            ForensicTestItemType.Blood => Resources.Blood,
            ForensicTestItemType.BulletShell => Resources.BulletShell,
            _ => forensicTestItemType.ToString(),
        };
    }
}