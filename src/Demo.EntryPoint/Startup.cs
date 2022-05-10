using Demo.Database.Context.Extensions;
using Demo.Database.Repositories.Extensions;

namespace Demo.EntryPoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTimescaleDb(Configuration);
            services.AddRepositories();
        }

        public void Configure(IApplicationBuilder _)
        {

        }
    }
}
