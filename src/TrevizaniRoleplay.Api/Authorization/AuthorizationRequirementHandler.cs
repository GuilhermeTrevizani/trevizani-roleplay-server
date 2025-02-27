using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrevizaniRoleplay.Domain.Enums;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Api.Authorization;

public class AuthorizationRequirementHandler(
    DatabaseContext databaseContext
        ) : AuthorizationHandler<AuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement)
    {
        var id = context.User?.Claims?.FirstOrDefault(x => x.Type == "Id")?.Value;
        if (string.IsNullOrWhiteSpace(id))
            return;

        var user = await databaseContext.Users.FirstOrDefaultAsync(x => x.Id == new Guid(id));
        if (user is null)
            return;

        if (requirement.UserStaff is not null
            && user.Staff < requirement.UserStaff)
            return;

        if (requirement.StaffFlag is not null
            && !JsonSerializer.Deserialize<StaffFlag[]>(user.StaffFlagsJSON)!.Contains(requirement.StaffFlag.Value))
            return;

        var banishment = await databaseContext.Banishments
            .Include(x => x.StaffUser)
            .FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (banishment is not null)
        {
            if (!banishment.ExpirationDate.HasValue || DateTime.Now <= banishment.ExpirationDate)
                return;

            databaseContext.Banishments.Remove(banishment);
            await databaseContext.SaveChangesAsync();
        }

        context.Succeed(requirement);
    }
}