namespace TrevizaniRoleplay.Server;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string command, string helpText = "") : Attribute
{
    public readonly string Command = command;
    public readonly string HelpText = helpText;

    public string[] Aliases { get; set; } = [];
    public bool GreedyArg { get; set; }
    public bool AllowEmptyStrings { get; set; }
}