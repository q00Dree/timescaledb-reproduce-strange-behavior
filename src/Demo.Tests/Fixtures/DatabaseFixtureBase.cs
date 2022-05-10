using Demo.Database.Context;
using Demo.Database.Context.Extensions;
using Demo.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Reflection;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Tests.Fixutres
{
    public abstract class DatabaseFixtureBase : IAsyncLifetime
    {
        public const int AdjustForChunkInPast = -40;

        public readonly string ConnectionString;
        public readonly DbContextOptions<TimescaleContext> DefaultOptions;
        public readonly TimeEventDataHypertableSharedResource SharedResource;

        public DatabaseFixtureBase()
        {
            Configuration? configuration = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            ConnectionString = configuration.ConnectionStrings.ConnectionStrings["TestDbConnection"].ConnectionString;

            DefaultOptions = new DbContextOptionsBuilder<TimescaleContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            SharedResource = new TimeEventDataHypertableSharedResource();
        }

        public abstract Task CleanupTimeEventDataHypertableAsync(TimescaleContext context);

        public async Task InitializeAsync()
        {
            await using var ctx = new TimescaleContext(DefaultOptions);
            await ctx.Database.MigrateAsync();

            await CleanupTimeEventDataHypertableAsync(ctx);
        }

        public async Task DisposeAsync()
        {
            await using var ctx = new TimescaleContext(DefaultOptions);

            await CleanupTimeEventDataHypertableAsync(ctx);
        }

        public async Task AddTimeEventDataToDatabaseAsync(TimeEventData ted,
            bool withCompressingChunk,
            DbContextOptions<TimescaleContext>? diagnosticContextOptions = null,
            ITestOutputHelper? outputHelper = null)
        {
            await SharedResource.Semaphore.WaitAsync();

            try
            {
                await using var context = new TimescaleContext(diagnosticContextOptions ?? DefaultOptions);

                await context.TimeEventsData.AddAsync(ted);
                outputHelper?.WriteLine(context.ChangeTracker.DebugView.LongView);

                await context.SaveChangesAsync();
                outputHelper?.WriteLine(context.ChangeTracker.DebugView.LongView);

                if (withCompressingChunk)
                {
                    await context.CompressChunkAsync(SharedResource.HypertableName, ted.Timestamp);
                }
            }
            finally
            {
                SharedResource.Semaphore.Release();
            }
        }

        public static TimeEventData CreateTimeEventData(Guid sourceId,
            Guid eventId,
            DateTimeOffset timestamp,
            int? dataId = null)
        {
            string jsonData = dataId is null ? "{\"DataId\": \"undefined\"}" : JsonSerializer.Serialize(new { DataId = dataId });

            return new TimeEventData(sourceId: sourceId,
                eventId: eventId,
                longitude: 0.0001,
                latitude: 0.0001,
                timestamp: timestamp,
                type: "double",
                jsonData: jsonData);
        }
    }
}
