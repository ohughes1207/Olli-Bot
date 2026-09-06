using System.Collections.Concurrent;

namespace OlliBot.Bot;
public sealed class KeyedSemaphore<TKey> : IKeyedSemaphore<TKey>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _semaphores = new();

    public IDisposable? TryAcquire(TKey key)
    {
        SemaphoreSlim semaphore = _semaphores.GetOrAdd(
            key,
            static _ => new SemaphoreSlim(1, 1));

        if (!semaphore.Wait(0))
            return null;

        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                semaphore.Release();
        }
    }
}