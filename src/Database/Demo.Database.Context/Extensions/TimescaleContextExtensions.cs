﻿using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Demo.Database.Context.Extensions
{
    public static class TimescaleContextExtensions
    {
        public static Task DecompressChunksAsync(this TimescaleContext context,
            string hypertableName,
            DateTimeOffset? rangeStart = null,
            DateTimeOffset? rangeEnd = null)
        {
            rangeStart ??= DateTimeOffset.MinValue;
            rangeEnd ??= DateTimeOffset.MaxValue;

            string query = $@"SELECT decompress_chunk(CONCAT('_timescaledb_internal.', chunk_name), if_compressed => true)::varchar 
                              FROM timescaledb_information.chunks 
                              WHERE hypertable_name = @hypertableName 
                              AND
                              (
                                  (@rangeStart BETWEEN range_start AND range_end) OR
                                  (@rangeEnd BETWEEN range_start AND range_end) OR
                                  (range_start > @rangeStart AND range_end < @rangeEnd) 
                              );";

            return context.Connection.ExecuteAsync(query, new { hypertableName, rangeStart, rangeEnd });
        }

        public static Task DecompressChunkAsync(this TimescaleContext context,
            string chunkName)
        {
            string query = $"SELECT decompress_chunk(CONCAT('_timescaledb_internal.', @chunkName), if_compressed => true)::varchar;";

            return context.Connection.ExecuteAsync(query, new { chunkName });
        }

        public static Task CompressChunkAsync(this TimescaleContext context,
            string hypertableName,
            DateTimeOffset timestamp)
        {
            string query = $@"SELECT compress_chunk(CONCAT('_timescaledb_internal.', chunk_name), if_not_compressed => true)::varchar 
                              FROM timescaledb_information.chunks 
                              WHERE hypertable_name = @hypertableName 
                              AND
                              (
                                  (@timestamp BETWEEN range_start AND range_end)
                              );";

            return context.Connection.ExecuteAsync(query, new { hypertableName, timestamp });
        }

        public static Task FindsAndPauseCompressionPolicyJobAsync(this TimescaleContext context,
            string hypertableName)
        {
            string query = $@"SELECT alter_job(job_id => 
                              (
                                  SELECT s.job_id 
                                  FROM timescaledb_information.jobs j 
                                  INNER JOIN timescaledb_information.job_stats s ON j.job_id = s.job_id 
                                  WHERE j.proc_name = 'policy_compression' 
                                  AND s.hypertable_name = @hypertableName 
                              ), 
                              scheduled => false, next_start => 'infinity');";

            return context.Connection.ExecuteAsync(query, new { hypertableName });
        }

        public static Task FindsAndScheduleCompressionPolicyJobAsync(this TimescaleContext context,
            string hypertableName)
        {
            string query = $@"SELECT alter_job(job_id => 
                              (
                                  SELECT s.job_id 
                                  FROM timescaledb_information.jobs j 
                                  INNER JOIN timescaledb_information.job_stats s ON j.job_id = s.job_id 
                                  WHERE j.proc_name = 'policy_compression' 
                                  AND s.hypertable_name = @hypertableName 
                              ), 
                              scheduled => true, next_start => 'now');";

            return context.Connection.ExecuteAsync(query, new { hypertableName });
        }

        public static Task DropChunksNewerThanAsync(this TimescaleContext context,
            string hypertableName,
            DateTimeOffset startPoint)
        {
            string query = $"SELECT drop_chunks(@hypertableName, newer_than => @startPoint);";

            return context.Connection.ExecuteAsync(query, new { hypertableName, startPoint });
        }

        public static Task DropChunksNewerThanNegativeInfinityAsync(this TimescaleContext context,
                    string hypertableName)
        {
            return DropChunksNewerThanAsync(context, hypertableName, DateTimeOffset.MinValue);
        }


        public static Task TruncateTableAsync(this TimescaleContext context,
            string hypertableName)
        {
            string query = $"TRUNCATE TABLE {hypertableName}";

            return context.Database.ExecuteSqlRawAsync(query);
        }
    }
}
