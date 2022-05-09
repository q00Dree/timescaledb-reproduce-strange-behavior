using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Database.Context.Extensions
{
    public static class TimescaleProviderExtension
    {
        public static void AddTimescaleDb(this IServiceCollection services, IConfiguration configuration)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<TimescaleContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddSingleton<TimeEventDataHypertableSharedResource>();
        }
    }
}
