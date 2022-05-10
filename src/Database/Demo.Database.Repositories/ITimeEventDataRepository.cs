using Demo.Database.Models;

namespace Demo.Database.Repositories
{
    public interface ITimeEventDataRepository
    {
        Task<List<TimeEventData>> GetTimeEventsDataBySourceIdAsync(Guid sourceId,
            DateTimeOffset? rangeStart = null,
            DateTimeOffset? rangeEnd = null,
            int? offset = null,
            int? limit = null);

        Task UpdateTimeEventDataAsync(TimeEventData updatedTimeEventData);
    }
}
