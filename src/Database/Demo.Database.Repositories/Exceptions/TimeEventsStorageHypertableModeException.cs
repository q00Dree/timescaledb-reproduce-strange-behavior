namespace Demo.Database.Repositories.Exceptions
{
    public class TimeEventsStorageHypertableModeException : Exception
    {
        public string HypertableName { get; private set; }

        public TimeEventsStorageHypertableModeException(string hypertableName,
            string? message)
            : base(message)
        {
            HypertableName = hypertableName;
        }

        public TimeEventsStorageHypertableModeException(string hypertableName,
            string? message,
            Exception innerException)
            : base(message, innerException)
        {
            HypertableName = hypertableName;
        }
    }
}
