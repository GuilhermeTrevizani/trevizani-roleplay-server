using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class HelpRequestTypeExtensions
{
    public static string GetDescription(this HelpRequestType helpRequestType)
    {
        return helpRequestType.ToString();
    }
}