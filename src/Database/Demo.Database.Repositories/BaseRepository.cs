using Demo.Database.Context;

namespace Demo.Database.Repositories
{
    public class BaseRepository : IDisposable
    {
        protected readonly TimescaleContext _dbContext;
        private bool _disposedValue;

        public BaseRepository(TimescaleContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
