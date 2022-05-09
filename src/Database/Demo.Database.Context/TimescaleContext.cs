using Demo.Database.Context.Configuring;
using Demo.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Demo.Database.Context
{
#nullable disable
    public class TimescaleContext : DbContext
    {
        public DbSet<TimeEventData> TimeEventsData { get; set; }

        public DbConnection Connection => Database.GetDbConnection();

        public TimescaleContext(DbContextOptions<TimescaleContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("timescaledb");

            modelBuilder.ApplyConfiguration(new TimeEventDataConfiguration());
        }
    }
#nullable restore
}
