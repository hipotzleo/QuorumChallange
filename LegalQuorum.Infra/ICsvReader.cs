namespace LegalQuorum.Infra
{
    public interface ICsvReader<T>
    {
        IAsyncEnumerable<T> ReadAsync(string path, CancellationToken ct = default);
    }
}
