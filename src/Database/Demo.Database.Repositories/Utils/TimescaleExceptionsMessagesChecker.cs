namespace Demo.Database.Repositories.Utils
{
    public static class TimescaleExceptionsMessagesChecker
    {
        public static bool CheckInsertion(string exceptionMessage)
        {
            if (exceptionMessage.Contains("insert into a compressed chunk that has primary or unique constraint is not supported"))
            {
                return true;
            }

            return false;
        }

        public static bool CheckModification(string exceptionMessage, out string chunkName)
        {
            if (exceptionMessage.Contains("XX000: cannot update/delete rows") && exceptionMessage.Contains("as it is compressed"))
            {
                chunkName = exceptionMessage.Replace("XX000: cannot update/delete rows from chunk ", string.Empty)
                    .Replace(" as it is compressed", string.Empty)
                    .Replace("\"", string.Empty);

                return true;
            }

            chunkName = string.Empty;
            return false;
        }
    }
}
