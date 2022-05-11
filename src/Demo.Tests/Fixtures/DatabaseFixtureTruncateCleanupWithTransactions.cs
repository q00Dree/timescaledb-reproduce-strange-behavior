using Demo.Database.Context;
using Demo.Database.Context.Extensions;
using Demo.Database.Models;
using Demo.Tests.Fixutres;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Tests.Fixtures
{
    public class DatabaseFixtureTruncateCleanupWithTransactions : DatabaseFixtureBase, IAsyncLifetime
    {
        public DatabaseFixtureTruncateCleanupWithTransactions() : base()
        {
        }

        #region CleanupTimeEventDataHypertableAsync
        public override async Task CleanupTimeEventDataHypertableAsync(TimescaleContext context)
        {
            await SharedResource.Semaphore.WaitAsync();

            try
            {
                await CleanupTimeEventDataHypertableWithRetriesAsync(context);
            }
            finally
            {
                SharedResource.Semaphore.Release();
            }
        }

        private async Task CleanupTimeEventDataHypertableWithRetriesAsync(TimescaleContext context,
            int retriesLimit = 5)
        {
            int retriesCounter = 0;

            while (retriesCounter < retriesLimit)
            {
                await using var transaction = context.Database.BeginTransaction();

                try
                {
                    await context.DecompressChunksAsync(SharedResource.HypertableName);
                    await context.TruncateTableAsync(SharedResource.HypertableName);

                    await transaction.CommitAsync();

                    return;
                }
                catch (Exception ex)
                {
                    if (retriesCounter == retriesLimit)
                    {
                        throw new Exception($"Something went wrong while cleaning up \'{SharedResource.HypertableName}\' hypertable.", ex);
                    }

                    retriesCounter++;
                }
            }
        }
        #endregion

        #region AddTimeEventDataToDatabaseAsync
        public override async Task AddTimeEventDataToDatabaseAsync(TimeEventData ted,
            bool withCompressingChunk,
            DbContextOptions<TimescaleContext>? diagnosticContextOptions = null,
            ITestOutputHelper? outputHelper = null)
        {
            await SharedResource.Semaphore.WaitAsync();

            try
            {
                await using var context = new TimescaleContext(diagnosticContextOptions ?? DefaultOptions);

                await AddTimeEventDataToTestDatabaseWithRetriesAsync(context, ted, withCompressingChunk);
            }
            finally
            {
                SharedResource.Semaphore.Release();
            }
        }

        private async Task AddTimeEventDataToTestDatabaseWithRetriesAsync(TimescaleContext context,
            TimeEventData ted,
            bool withCompressingChunk,
            int retriesLimit = 5,
            ITestOutputHelper? outputHelper = null)
        {
            int retriesCounter = 0;

            while (retriesCounter < retriesLimit)
            {
                await using var transaction = context.Database.BeginTransaction();

                try
                {
                    await context.TimeEventsData.AddAsync(ted);
                    outputHelper?.WriteLine(context.ChangeTracker.DebugView.LongView);

                    await context.SaveChangesAsync();
                    outputHelper?.WriteLine(context.ChangeTracker.DebugView.LongView);

                    if (withCompressingChunk)
                    {
                        await context.CompressChunkAsync(SharedResource.HypertableName, ted.Timestamp);
                    }

                    await transaction.CommitAsync();

                    return;
                }
                catch (Exception ex)
                {
                    if (retriesCounter == retriesLimit)
                    {
                        throw new Exception($"Something went wrong while adding test TimeEventData to \'{SharedResource.HypertableName}\' hypertable.", ex);
                    }

                    retriesCounter++;
                }
            }
        }
        #endregion
    }
}
