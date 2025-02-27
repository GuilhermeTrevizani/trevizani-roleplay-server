namespace TrevizaniRoleplay.Server.Extensions;

public static class StringExtension
{
    public static Guid? ToGuid(this string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : new Guid(value);
    }
}