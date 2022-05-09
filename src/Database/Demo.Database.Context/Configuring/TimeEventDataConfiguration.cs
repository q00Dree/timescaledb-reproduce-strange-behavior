using Demo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Demo.Database.Context.Configuring
{
    internal class TimeEventDataConfiguration : IEntityTypeConfiguration<TimeEventData>
    {
        public void Configure(EntityTypeBuilder<TimeEventData> builder)
        {
            builder.ToTable("timeeventsdata");

            builder.HasKey(timeEventData => new { timeEventData.SourceId, timeEventData.Timestamp });

            builder.HasIndex(timeEventData => timeEventData.EventId);

            builder.Property(timeEventData => timeEventData.SourceId)
                .HasColumnName("sourceid");

            builder.Property(timeEventData => timeEventData.EventId)
                .HasColumnName("eventid");

            builder.Property(timeEventData => timeEventData.Latitude)
                .HasColumnName("latitude");

            builder.Property(timeEventData => timeEventData.Longitude)
                .HasColumnName("longitude");

            builder.Property(timeEventData => timeEventData.JsonData)
                .HasColumnName("jsondata")
                .HasColumnType("json")
                .IsRequired();

            builder.Property(timeEventData => timeEventData.Type)
                .HasColumnName("type")
                .IsRequired();

            builder.Property(timeEventData => timeEventData.Timestamp)
                .HasColumnName("timestamp")
                .HasColumnType("timestamp with time zone");
        }
    }
}
