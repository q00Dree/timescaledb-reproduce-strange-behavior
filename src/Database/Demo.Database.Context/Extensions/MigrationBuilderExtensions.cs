using Microsoft.EntityFrameworkCore.Migrations;

namespace Demo.Database.Context.Extensions
{
    public static class TimescaleMigrationBuilderExtensions
    {
        public static MigrationBuilder ConvertToHypertable(this MigrationBuilder migrationBuilder,
            string tableName,
            string timeColumnName)
        {
            string query = $"SELECT create_hypertable(relation => '{tableName}', time_column_name => '{timeColumnName}', if_not_exists => true);";

            migrationBuilder.Sql(query);

            return migrationBuilder;
        }

        public static MigrationBuilder EnableCompressionOnHypertable(this MigrationBuilder migrationBuilder,
            string hypertableName,
            string segmentByColumnName,
            string orderByColumnName)
        {
            string query = $@"ALTER TABLE {hypertableName} SET
                              (
                                  timescaledb.compress = true,
                                  timescaledb.compress_segmentby = '{segmentByColumnName}',
                                  timescaledb.compress_orderby = '{orderByColumnName}'
                              );";

            migrationBuilder.Sql(query);

            return migrationBuilder;
        }

        public static MigrationBuilder AddCompressionPolicy(this MigrationBuilder migrationBuilder,
            string hypertableName,
            int compressAfter)
        {
            string query = $"SELECT add_compression_policy(hypertable => '{hypertableName}', compress_after => INTERVAL '{compressAfter}d');";

            migrationBuilder.Sql(query);

            return migrationBuilder;
        }

        public static MigrationBuilder AddRetentionPolicy(this MigrationBuilder migrationBuilder,
            string hypertableName,
            int dropAfter)
        {
            string query = $"SELECT add_retention_policy(relation => '{hypertableName}', drop_after => INTERVAL '{dropAfter}d');";

            migrationBuilder.Sql(query);

            return migrationBuilder;
        }
    }
}
