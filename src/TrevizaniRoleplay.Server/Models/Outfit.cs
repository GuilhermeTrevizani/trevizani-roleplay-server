namespace TrevizaniRoleplay.Server.Models;

public class Outfit
{
    public Outfit()
    {
    }

    public Outfit(int slot, short cloth4Drawable, short cloth6Drawable)
    {
        Slot = slot;
        Cloth4 = new() { Drawable = cloth4Drawable };
        Cloth6 = new() { Drawable = cloth6Drawable };
    }

    public int Slot { get; set; }
    public ClothAccessory Cloth1 { get; set; } = new();
    public ClothAccessory Cloth3 { get; set; } = new() { Drawable = Constants.CLOTH_3_8_11_DEFAULT_DRAWABLE };
    public ClothAccessory Cloth4 { get; set; } = new();
    public ClothAccessory Cloth5 { get; set; } = new();
    public ClothAccessory Cloth6 { get; set; } = new();
    public ClothAccessory Cloth7 { get; set; } = new();
    public ClothAccessory Cloth8 { get; set; } = new() { Drawable = Constants.CLOTH_3_8_11_DEFAULT_DRAWABLE };
    public ClothAccessory Cloth9 { get; set; } = new();
    public ClothAccessory Cloth10 { get; set; } = new();
    public ClothAccessory Cloth11 { get; set; } = new() { Drawable = Constants.CLOTH_3_8_11_DEFAULT_DRAWABLE };
    public ClothAccessory Accessory0 { get; set; } = new() { Drawable = -1 };
    public ClothAccessory Accessory1 { get; set; } = new() { Drawable = -1 };
    public ClothAccessory Accessory2 { get; set; } = new() { Drawable = -1 };
    public ClothAccessory Accessory6 { get; set; } = new() { Drawable = -1 };
    public ClothAccessory Accessory7 { get; set; } = new() { Drawable = -1 };

    public class ClothAccessory()
    {
        public short Drawable { get; set; }
        public byte Texture { get; set; }
        public string? DLC { get; set; }
        public bool Using { get; set; } = true;
    }
}