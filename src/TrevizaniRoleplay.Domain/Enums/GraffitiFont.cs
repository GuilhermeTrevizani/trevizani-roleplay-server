using System.ComponentModel.DataAnnotations;

namespace TrevizaniRoleplay.Domain.Enums;

public enum GraffitiFont : byte
{
    [Display(Name = "a Attack Graffiti")]
    Attack = 1,

    [Display(Name = "Barrio Santo")]
    BarrioSanto = 2,

    [Display(Name = "Wildside")]
    Wildside = 3,

    [Display(Name = "Califas Demo")]
    CalifasDemo = 4,

    [Display(Name = "Fat Wandals PERSONAL USE")]
    FatWandals_PERSONAL = 5,

    [Display(Name = "Fat Wandals Alt PERSONAL USE")]
    FatWandalsAlt_PERSONAL = 6,

    [Display(Name = "Fat Wandals Element PERSONAL")]
    FatWandalsElement_PERSONAL = 7,

    [Display(Name = "Hesorder")]
    Hesorder = 8,

    [Display(Name = "Jamstreet Graffiti")]
    JamstreetGraffiti = 9,

    [Display(Name = "Next Ups")]
    NextUps = 10,

    [Display(Name = "Timegoing")]
    TimegoingRegular = 11,

    [Display(Name = "Tagster")]
    TAGSTER = 12,

    [Display(Name = "Phantom Urbanism")]
    PhantomUrbanism = 13,

    [Display(Name = "Slim Wandals PERSONAL USE")]
    SlimWandals_PERSONAL = 14,

    [Display(Name = "Slim Wandals Alt PERSONAL USE")]
    SlimWandalsAlt_PERSONAL = 15,

    [Display(Name = "Slim Wandals Element PERSONAL")]
    SlimWandalsElement_PERSONAL = 16,

    [Display(Name = "Subway")]
    Subway = 17,

    [Display(Name = "SR CUEN font")]
    srcuen_font = 18,

    [Display(Name = "Streetlife Personal use")]
    StreetLife = 19,

    [Display(Name = "Street Life")]
    StreetLife2 = 20,
}