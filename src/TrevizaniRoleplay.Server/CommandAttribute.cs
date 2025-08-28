namespace TrevizaniRoleplay.Server;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string[] commands, string category, string description, string helpText = "") : Attribute
{
    public string[] Commands { get; } = commands;
    public string Category { get; } = category;
    public string Description { get; } = description;
    public string HelpText { get; } = helpText;
    public bool GreedyArg { get; set; }
    public bool AllowEmptyStrings { get; set; }
}