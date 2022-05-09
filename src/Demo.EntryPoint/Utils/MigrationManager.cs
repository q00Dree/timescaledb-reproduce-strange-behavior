using Demo.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Demo.EntryPoint.Utils
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost webHost)
        {
            using var serviceScope = webHost.Services.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<TimescaleContext>()!;

            context.Database.Migrate();
            return webHost;
        }
    }
}
