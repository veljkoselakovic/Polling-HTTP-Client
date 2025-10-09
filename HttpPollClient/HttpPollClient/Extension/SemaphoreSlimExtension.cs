namespace HttpPollClient.Extension
{
    public static class SemaphoreSlimExtensions
    {
        public static async Task<SemaphoreAutoReleaser> WaitAsyncWithAutoReleaser(
            this SemaphoreSlim self
        )
        {
            await self.WaitAsync();
            return new SemaphoreAutoReleaser(self);
        }

        public static SemaphoreAutoReleaser WaitWithAutoReleaser(this SemaphoreSlim self)
        {
            self.Wait();
            return new SemaphoreAutoReleaser(self);
        }

        public sealed class SemaphoreAutoReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _disposed;

            public SemaphoreAutoReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore ?? throw new ArgumentException(nameof(semaphore));
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _semaphore.Release();
                }
                catch (Exception e) { }

                _disposed = true;
            }
        }
    }
}
