namespace Demo.Database.Repositories.Exceptions
{
    public class TimeEventsStorageNotFoundException : Exception
    {
        public TimeEventsStorageNotFoundException()
        {
        }

        public TimeEventsStorageNotFoundException(string? message)
            : base(message)
        {
        }

        public TimeEventsStorageNotFoundException(string? message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
