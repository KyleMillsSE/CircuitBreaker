namespace CircuitBreaker.Common
{
    public class DisposableLock : IDisposable
    {
        private readonly object _lock;

        private DisposableLock(object loc)
        {
            _lock = loc;
        }

        public static DisposableLock Lock(object loc)
        {
            var dl = new DisposableLock(loc);

            if (!Monitor.TryEnter(loc))
            {
                throw new Exception("i should never be thrown");
            };

            return dl;
        }

        public void Dispose()
        {
            Monitor.Exit(_lock);
        }
    }
}
