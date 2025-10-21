using LegalQuorum.Domain.Models;
using LegalQuorum.Infra.CsvSetup;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LegalQuorum.Infra.Cache;

public sealed class DataCache
{
    private readonly ILogger<DataCache> _logger;
    private readonly DataPaths _paths;
    private readonly IMemoryCache _cache;
    private readonly ICsvReader<Legislator> _peopleReader;
    private readonly ICsvReader<Bill> _billReader;
    private readonly ICsvReader<Vote> _voteReader;
    private readonly ICsvReader<VoteResult> _voteResultReader;

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        // revalida automaticamente após 10 min sem uso (ajuste à vontade)
        SlidingExpiration = TimeSpan.FromMinutes(10),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private const string PeopleKey = "csv:people";
    private const string BillsKey = "csv:bills";
    private const string VotesKey = "csv:votes";
    private const string ResultsKey = "csv:vote_results";

    public DataCache(
        ILogger<DataCache> logger,
        IOptions<DataPaths> options,
        IMemoryCache cache,
        ICsvReader<Legislator> peopleReader,
        ICsvReader<Bill> billReader,
        ICsvReader<Vote> voteReader,
        ICsvReader<VoteResult> voteResultReader)
    {
        _logger = logger;
        _paths = options.Value;
        _cache = cache;
        _peopleReader = peopleReader;
        _billReader = billReader;
        _voteReader = voteReader;
        _voteResultReader = voteResultReader;
    }

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(PeopleKey, out List<Legislator>? _)) return;

        var people = await LoadAsync(_paths.People, _peopleReader, "people", ct);
        var bills = await LoadAsync(_paths.Bills, _billReader, "bills", ct);
        var votes = await LoadAsync(_paths.Votes, _voteReader, "votes", ct);
        var res = await LoadAsync(_paths.VoteResults, _voteResultReader, "vote_results", ct);

        _cache.Set(PeopleKey, people, CacheOptions);
        _cache.Set(BillsKey, bills, CacheOptions);
        _cache.Set(VotesKey, votes, CacheOptions);
        _cache.Set(ResultsKey, res, CacheOptions);

        _logger.LogInformation("CSV loaded: people={People}, bills={Bills}, votes={Votes}, results={Results}",
            people.Count, bills.Count, votes.Count, res.Count);
    }

    public (IReadOnlyList<Legislator> people, IReadOnlyList<Bill> bills, IReadOnlyList<Vote> votes, IReadOnlyList<VoteResult> results) Snapshot()
    {
        return (
            _cache.Get<List<Legislator>>(PeopleKey) ?? new(),
            _cache.Get<List<Bill>>(BillsKey) ?? new(),
            _cache.Get<List<Vote>>(VotesKey) ?? new(),
            _cache.Get<List<VoteResult>>(ResultsKey) ?? new()
        );
    }

    public void Invalidate()
    {
        _cache.Remove(PeopleKey);
        _cache.Remove(BillsKey);
        _cache.Remove(VotesKey);
        _cache.Remove(ResultsKey);
    }

    private async Task<List<T>> LoadAsync<T>(string path, ICsvReader<T> reader, string label, CancellationToken ct)
    {
        var list = new List<T>();
        try
        {
            await foreach (var item in reader.ReadAsync(path, ct))
                list.Add(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail at reading the CSV '{Label}' in {Path}. Returning parcial list with {Count} items.", label, path, list.Count);
        }
        _logger.LogInformation("Loaded '{Label}': {Count} items (from {Path})", label, list.Count, path);
        return list;
    }
}
