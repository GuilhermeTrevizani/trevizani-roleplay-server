using System.ComponentModel.DataAnnotations;

namespace TrevizaniRoleplay.Core.Extesions;

public static class EnumExtensions
{
    [Obsolete("Each Enum will have its own GetDescription extension method")]
    public static string GetDisplay(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null)
            return string.Empty;

        var attributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
        return attributes.FirstOrDefault()?.Name ?? value.ToString();
    }
}