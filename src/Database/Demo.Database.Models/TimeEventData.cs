namespace Demo.Database.Models
{
    public class TimeEventData
    {
        public Guid SourceId { get; set; }

        public Guid EventId { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string Type { get; set; }

        public string JsonData { get; set; }

        public TimeEventData(Guid sourceId,
            Guid eventId,
            double? longitude,
            double? latitude,
            DateTimeOffset timestamp,
            string type,
            string jsonData)
        {
            SourceId = sourceId;
            EventId = eventId;
            Longitude = longitude;
            Latitude = latitude;
            Timestamp = timestamp;
            Type = type;
            JsonData = jsonData;
        }

        public TimeEventData(double longitude, double latitude, string type, string jsonData)
            : this(Guid.NewGuid(), Guid.NewGuid(), longitude, latitude, DateTimeOffset.UtcNow, type, jsonData)
        {
        }
    }
}
