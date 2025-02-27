using System.ComponentModel.DataAnnotations;

namespace TrevizaniRoleplay.Domain.Enums;

public enum UserReceiveSMSDiscord : byte
{
    [Display(Name = "Todos")]
    All = 1,

    [Display(Name = "Apenas individuais")]
    OnlyIndividuals = 2,

    [Display(Name = "Nenhum")]
    None = 3,
}