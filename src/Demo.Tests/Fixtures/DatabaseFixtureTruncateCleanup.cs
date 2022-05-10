using Demo.Database.Context;
using Demo.Database.Context.Extensions;
using Xunit;

namespace Demo.Tests.Fixutres
{
    public class DatabaseFixtureTruncateCleanup : DatabaseFixtureBase, IAsyncLifetime
    {
        public DatabaseFixtureTruncateCleanup() : base()
        {
        }

        public override async Task CleanupTimeEventDataHypertableAsync(TimescaleContext context)
        {
            await SharedResource.Semaphore.WaitAsync();

            try
            {
                await context.DecompressChunksAsync(SharedResource.HypertableName);
                await context.TruncateTableAsync(SharedResource.HypertableName);
            }
            finally
            {
                SharedResource.Semaphore.Release();
            }
        }
    }
}
