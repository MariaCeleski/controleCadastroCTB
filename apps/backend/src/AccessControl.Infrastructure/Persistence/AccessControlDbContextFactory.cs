using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccessControl.Infrastructure.Persistence;

// EF tooling uses this factory without starting the API or requiring runtime secrets.
public sealed class AccessControlDbContextFactory : IDesignTimeDbContextFactory<AccessControlDbContext>
{
    public AccessControlDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ACCESS_CONTROL_CONNECTION_STRING")
            ?? "Host=localhost;Database=access_control;Username=access_control;Password=design_time_only";
        return new AccessControlDbContext(new DbContextOptionsBuilder<AccessControlDbContext>().UseNpgsql(connectionString).Options);
    }
}
