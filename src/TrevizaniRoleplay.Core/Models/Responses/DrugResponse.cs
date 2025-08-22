using TrevizaniRoleplay.Core.Models.Requests;

namespace TrevizaniRoleplay.Core.Models.Responses;

public class DrugResponse : DrugRequest
{
    public string Name { get; set; } = default!;
}