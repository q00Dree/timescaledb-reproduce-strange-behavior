using Demo.Database.Context;
using Demo.Database.Context.Extensions;
using Demo.Database.Models;
using Demo.Database.Repositories.Exceptions;
using Demo.Database.Repositories.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demo.Database.Repositories
{
    public class TimeEventDataRepository : BaseRepository, ITimeEventDataRepository
    {
        private readonly TimeEventDataHypertableSharedResource _sharedResource;
        private readonly ILogger<TimeEventDataRepository> _logger;

        public TimeEventDataRepository(TimescaleContext context,
            TimeEventDataHypertableSharedResource timeEventDataHypertableSharedResource,
            ILogger<TimeEventDataRepository> logger)
            : base(context)
        {
            _sharedResource = timeEventDataHypertableSharedResource;
            _logger = logger;
        }

        public async Task<List<TimeEventData>> GetTimeEventsDataBySourceIdAsync(Guid sourceId,
            DateTimeOffset? rangeStart = null,
            DateTimeOffset? rangeEnd = null,
            int? offset = null,
            int? limit = null)
        {
            rangeStart ??= DateTimeOffset.MinValue;
            rangeEnd ??= DateTimeOffset.UtcNow;

            offset ??= 0;
            limit ??= int.MaxValue;

            if (rangeStart > rangeEnd)
            {
                throw new ArgumentException($"Argument [RangeStart: {rangeStart}] can't be later than [RangeEnd: {rangeEnd}]!");
            }

            List<TimeEventData> storedTeds = await _dbContext.TimeEventsData
                .Where(ted => ted.SourceId == sourceId && ted.Timestamp >= rangeStart && ted.Timestamp <= rangeEnd)
                .OrderBy(ted => ted.SourceId)
                .ThenByDescending(ted => ted.Timestamp)
                .Skip(offset.Value)
                .Take(limit.Value)
                .AsNoTracking()
                .ToListAsync();

            return storedTeds;
        }

        public async Task UpdateTimeEventDataAsync(TimeEventData updatedTed)
        {
            if (Interlocked.Read(ref _sharedResource.IsNormalMode) == 1)
            {
                Interlocked.Increment(ref _sharedResource.WorkingThreadsCounter);

                await _sharedResource.Semaphore.WaitAsync();

                try
                {
                    TimeEventData? storedTrackingTed = await _dbContext.TimeEventsData
                        .FirstOrDefaultAsync(d => d.SourceId == updatedTed.SourceId && d.Timestamp == updatedTed.Timestamp);

                    // Usually tests fall here, because entity with PK (SourceId + Timestamp) isn't in db, but have to be.
                    if (storedTrackingTed is null)
                    {
                        throw new TimeEventsStorageNotFoundException(
                            $"Time event data with [SourceId: {updatedTed.SourceId} and Timestamp: {updatedTed.Timestamp}] is not exist!");
                    }

                    try
                    {
                        await UpdateTimeEventDataFromUncompressedChunkAsync(storedTrackingTed, updatedTed);
                    }
                    catch (Exception ex)
                    when (ex.InnerException != null && TimescaleExceptionsMessagesChecker.CheckModification(ex.InnerException.Message, out string chunkName))
                    {
                        _logger.LogInformation("Trying update time event data with [SourceId: {sourceId} and Timestamp: {timestamp}] in compressed chunk with [Name: '{chunkName}'].",
                            storedTrackingTed.SourceId, storedTrackingTed.Timestamp, chunkName);

                        await UpdateTimeEventDataFromCompressedChunkAsync(storedTrackingTed, updatedTed, chunkName);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _sharedResource.WorkingThreadsCounter);

                    _sharedResource.Semaphore.Release();
                }
            }
            else
            {
                throw new TimeEventsStorageHypertableModeException(_sharedResource.HypertableName,
                    $"Can't insert/update/remove data from hypertable '{_sharedResource.HypertableName}', because it's in configuration mode!");
            }
        }

        private async Task UpdateTimeEventDataFromUncompressedChunkAsync(TimeEventData trackingTed,
            TimeEventData updatedTed)
        {
            trackingTed.Latitude = updatedTed.Latitude;
            trackingTed.Longitude = updatedTed.Longitude;
            trackingTed.Type = updatedTed.Type;
            trackingTed.JsonData = updatedTed.JsonData;

            _dbContext.Update(trackingTed);
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateTimeEventDataFromCompressedChunkAsync(TimeEventData trackingTed,
            TimeEventData updatedTed,
            string compressedChunkName)
        {
            await _dbContext.FindsAndPauseCompressionPolicyJobAsync(_sharedResource.HypertableName);

            await _dbContext.DecompressChunkAsync(compressedChunkName);

            await UpdateTimeEventDataFromUncompressedChunkAsync(trackingTed, updatedTed);

            await _dbContext.FindsAndScheduleCompressionPolicyJobAsync(_sharedResource.HypertableName);
        }
    }
}
