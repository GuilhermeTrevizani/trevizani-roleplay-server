using TrevizaniRoleplay.Domain.Entities;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Core.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<Character> WhereActive(this IQueryable<Character> query, bool showDead = true)
    {
        if (!showDead)
            query = query.Where(x => !x.DeathDate.HasValue);

        return query.Where(x => x.NameChangeStatus != CharacterNameChangeStatus.Done && !x.DeletedDate.HasValue);
    }
}