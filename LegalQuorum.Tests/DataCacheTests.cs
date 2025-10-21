using FluentAssertions;
using LegalQuorum.Domain.Models;
using LegalQuorum.Infra.Cache;
using LegalQuorum.Infra.CsvSetup;
using LegalQuorum.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LegalQuorum.Tests;

public class DataCacheTests
{
    private static DataCache CreateCache(
        IEnumerable<Legislator> people,
        IEnumerable<Bill> bills,
        IEnumerable<Vote> votes,
        IEnumerable<VoteResult> results)
    {
        var options = Options.Create(new DataPaths
        {
            People = "people.csv",
            Bills = "bills.csv",
            Votes = "votes.csv",
            VoteResults = "vote_results.csv"
        });

        var cache = new MemoryCache(new MemoryCacheOptions());
        return new DataCache(
            new NullLogger<DataCache>(),
            options,
            cache,
            new FakeCsvReader<Legislator>(people),
            new FakeCsvReader<Bill>(bills),
            new FakeCsvReader<Vote>(votes),
            new FakeCsvReader<VoteResult>(results));
    }

    [Fact]
    public async Task Should_cache_after_first_load_and_return_snapshot()
    {
        var people = new[] { new Legislator { Id = 1, Name = "Alice" } };
        var bills = new[] { new Bill { Id = 10, Title = "Bill A", SponsorId = 1 } };
        var votes = new[] { new Vote { Id = 100, BillId = 10 } };
        var res = new[] { new VoteResult { Id = 1, LegislatorId = 1, VoteId = 100, VoteType = 1 } };

        var dc = CreateCache(people, bills, votes, res);

        await dc.EnsureLoadedAsync();
        var snap1 = dc.Snapshot();
        snap1.people.Should().HaveCount(1);

        dc.Invalidate();
        var snap2 = dc.Snapshot();
        snap2.people.Should().BeEmpty();

        await dc.EnsureLoadedAsync();
        var snap3 = dc.Snapshot();
        snap3.people.Should().HaveCount(1);
    }
}
