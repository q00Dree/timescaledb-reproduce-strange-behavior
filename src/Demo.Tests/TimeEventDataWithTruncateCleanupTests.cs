using Demo.Database.Context;
using Demo.Database.Models;
using Demo.Tests.Fixutres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Tests
{
    public class TimeEventDataWithTruncateCleanupTests : IClassFixture<DatabaseFixtureTruncateCleanup>, IAsyncLifetime
    {
        private readonly DatabaseFixtureTruncateCleanup _fixture;
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<TimescaleContext> _diagnosticOptions;
        private readonly TimescaleContext _context;

        public TimeEventDataWithTruncateCleanupTests(DatabaseFixtureTruncateCleanup fixture,
            ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;

            _diagnosticOptions = new DbContextOptionsBuilder<TimescaleContext>()
                .LogTo(_output.WriteLine, LogLevel.Debug)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseNpgsql(_fixture.ConnectionString)
                .Options;

            _context = new TimescaleContext(_diagnosticOptions);
        }

        #region IAsyncLifetime
        public async Task InitializeAsync()
        {
            await _fixture.CleanupTimeEventDataHypertableAsync(_context);
        }

        public async Task DisposeAsync()
        {
            _fixture.SharedResource.IsNormalMode = 1;

            await _fixture.CleanupTimeEventDataHypertableAsync(_context);
            await _context.DisposeAsync();
        }
        #endregion

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(20)]
        [InlineData(25)]
        [InlineData(30)]
        [InlineData(35)]
        [InlineData(40)]
        [InlineData(45)]
        [InlineData(50)]
        public async Task InsertTimeEventDataToDatabaseOkTest(int itemsToCreate)
        {
            // Arrange & Act
            var sourceId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var tedsToSave = new List<TimeEventData>(itemsToCreate);
            for (int i = 0; i < itemsToCreate; i++)
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(DatabaseFixtureBase.AdjustForChunkInPast * i);

                TimeEventData tedToSave = DatabaseFixtureBase.CreateTimeEventData(sourceId, eventId, timestamp, i + 1);
                tedsToSave.Add(tedToSave);

                await _fixture.AddTimeEventDataToDatabaseAsync(tedToSave, true, _diagnosticOptions, _output);
            }

            // Assert
            int storedTedsCount = _context.TimeEventsData.Count();

            if (itemsToCreate != storedTedsCount)
            {
                // set your debug breakpoint here and run tests until reach this point
                // see logs and database records at this moment
            }

            Assert.Equal(itemsToCreate, storedTedsCount);
        }
    }
}
