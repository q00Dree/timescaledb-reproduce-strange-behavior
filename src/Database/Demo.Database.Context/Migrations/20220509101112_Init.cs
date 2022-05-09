using Demo.Database.Context.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Database.Context.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:timescaledb", ",,");

            migrationBuilder.CreateTable(
                name: "timeeventsdata",
                columns: table => new
                {
                    sourceid = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eventid = table.Column<Guid>(type: "uuid", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    jsondata = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timeeventsdata", x => new { x.sourceid, x.timestamp });
                });

            migrationBuilder.CreateIndex(
                name: "IX_timeeventsdata_eventid",
                table: "timeeventsdata",
                column: "eventid");

            migrationBuilder.ConvertToHypertable("timeeventsdata", "timestamp")
                .EnableCompressionOnHypertable("timeeventsdata", "sourceid", "timestamp")
                .AddCompressionPolicy("timeeventsdata", 30)
                .AddRetentionPolicy("timeeventsdata", 365);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timeeventsdata");
        }
    }
}
