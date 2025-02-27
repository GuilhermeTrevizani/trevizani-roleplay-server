using System.ComponentModel.DataAnnotations;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Domain.Enums;

public enum CompanyFlag : byte
{
    [Display(Name = Globalization.INVITE_EMPLOYEE)]
    InviteEmployee = 1,

    [Display(Name = Globalization.EDIT_EMPLOYEE)]
    EditEmployee = 2,

    [Display(Name = Globalization.REMOVE_EMPLOYEE)]
    RemoveEmployee = 3,

    [Display(Name = Globalization.OPEN)]
    Open = 4,

    [Display(Name = Globalization.MANAGE_ITEMS)]
    ManageItems = 5,

    [Display(Name = Globalization.SAFE)]
    Safe = 6,

    [Display(Name = Globalization.FURNITURES)]
    Furnitures = 7,
}