using Demo.Database.Context;
using Demo.Database.Context.Extensions;
using Xunit;

namespace Demo.Tests.Fixutres
{
    public class DatabaseFixtureDropChunksCleanup : DatabaseFixtureBase, IAsyncLifetime
    {
        public DatabaseFixtureDropChunksCleanup() : base()
        {
        }

        public override async Task CleanupTimeEventDataHypertableAsync(TimescaleContext context)
        {
            await SharedResource.Semaphore.WaitAsync();

            try
            {
                await context.DropChunksNewerThanNegativeInfinityAsync(SharedResource.HypertableName);
            }
            finally
            {
                SharedResource.Semaphore.Release();
            }
        }
    }
}
