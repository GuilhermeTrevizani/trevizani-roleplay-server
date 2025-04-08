using Microsoft.EntityFrameworkCore.Design;
using TrevizaniRoleplay.Infra.Data;

namespace TrevizaniRoleplay.Server;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        return Functions.GetDatabaseContext();
    }
}