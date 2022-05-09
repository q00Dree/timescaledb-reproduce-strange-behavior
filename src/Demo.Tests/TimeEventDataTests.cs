using Demo.Database.Context;
using Demo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Demo.Tests
{
    [Collection("DatabaseCollection")]
    public class TimeEventDataTests : IAsyncLifetime
    {
        private readonly DatabaseFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<TimescaleContext> _diagnosticOptions;
        private readonly TimescaleContext _context;

        public TimeEventDataTests(DatabaseFixture fixture,
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
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(40)]
        [InlineData(80)]
        public async Task InsertTimeEventDataToDatabaseOkTest(int itemsToCreate)
        {
            // Arrange & Act
            var sourceId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var tedsToSave = new List<TimeEventData>();
            for (int i = 0; i < itemsToCreate; i++)
            {
                var timestamp = DateTimeOffset.UtcNow.AddDays(DatabaseFixture.AdjustForChunkInPast * i);

                TimeEventData tedToSave = DatabaseFixture.CreateTimeEventData(sourceId, eventId, timestamp, i + 1);
                tedsToSave.Add(tedToSave);

                await _fixture.AddTimeEventDataToDatabaseAsync(tedToSave, true, _diagnosticOptions, _output);
            }

            // Assert
            int storedTedsCount = _context.TimeEventsData.Count();

            Assert.Equal(tedsToSave.Count, storedTedsCount);
        }
    }
}
