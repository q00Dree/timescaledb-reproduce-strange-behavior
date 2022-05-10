using Microsoft.Extensions.DependencyInjection;

namespace Demo.Database.Repositories.Extensions
{
    public static class RepositoriesProviderExtension
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ITimeEventDataRepository, TimeEventDataRepository>();
        }
    }
}
