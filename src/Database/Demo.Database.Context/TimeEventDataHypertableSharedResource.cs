namespace Demo.Database.Context
{
    public class TimeEventDataHypertableSharedResource
    {
        public readonly string HypertableName;

        public long IsNormalMode;

        public long WorkingThreadsCounter;

        public readonly SemaphoreSlim Semaphore;

        public TimeEventDataHypertableSharedResource()
        {
            HypertableName = "timeeventsdata";

            IsNormalMode = 1;
            WorkingThreadsCounter = 0;

            Semaphore = new SemaphoreSlim(1, 1);
        }
    }
}
