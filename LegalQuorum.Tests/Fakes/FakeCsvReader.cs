using LegalQuorum.Infra;

namespace LegalQuorum.Tests.Fakes;

public sealed class FakeCsvReader<T> : ICsvReader<T>
{
    private readonly IEnumerable<T> _data;
    public FakeCsvReader(IEnumerable<T> data) => _data = data;

    public async IAsyncEnumerable<T> ReadAsync(string path, CancellationToken ct = default)
    {
        foreach (var item in _data)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
