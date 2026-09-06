namespace OlliBot.Bot;

public interface IKeyedSemaphore<TKey>
    where TKey : notnull
{
    IDisposable? TryAcquire(TKey key);
}