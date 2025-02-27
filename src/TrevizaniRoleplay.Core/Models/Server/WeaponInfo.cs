namespace TrevizaniRoleplay.Core.Models.Server;

public class WeaponInfo
{
    public string Name { get; set; } = string.Empty;
    public float Recoil { get; set; }
    public ushort Damage { get; set; }
    public Guid? AmmoItemTemplateId { get; set; }
    public string AmmoItemTemplateName { get; set; } = string.Empty;
    public bool AttachToBody { get; set; }
    public IEnumerable<Component> Components { get; set; } = [];

    public class Component
    {
        public Guid ItemTemplateId { get; set; }
        public string ItemTemplateName { get; set; } = string.Empty;
    }
}