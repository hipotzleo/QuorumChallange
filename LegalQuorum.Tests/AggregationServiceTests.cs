using FluentAssertions;
using LegalQuorum.Domain.Models;
using LegalQuorum.Infra.Cache;
using LegalQuorum.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using LegalQuorum.Infra.CsvSetup;
using LegalQuorum.Services;

namespace LegalQuorum.Tests;

public class AggregationServiceTests
{
    private static (AggregationService svc, DataCache cache) Create(
        IEnumerable<Legislator> people,
        IEnumerable<Bill> bills,
        IEnumerable<Vote> votes,
        IEnumerable<VoteResult> results)
    {
        var options = Options.Create(new DataPaths { People = "", Bills = "", Votes = "", VoteResults = "" });
        var mem = new MemoryCache(new MemoryCacheOptions());
        var dc = new DataCache(
            new NullLogger<DataCache>(),
            options,
            mem,
            new FakeCsvReader<Legislator>(people),
            new FakeCsvReader<Bill>(bills),
            new FakeCsvReader<Vote>(votes),
            new FakeCsvReader<VoteResult>(results));

        var svc = new AggregationService(dc);
        return (svc, dc);
    }

    [Fact]
    public void Should_aggregate_votes_and_map_sponsor_name()
    {
        var people = new[]
        {
            new Legislator { Id = 412211, Name = "Rep. John Yarmuth (D-KY-3)" }
        };

        var bills = new[]
        {
            new Bill { Id = 2952375, Title = "H.R. 5376: Build Back Better Act", SponsorId = 412211 },
            new Bill { Id = 2900994, Title = "H.R. 3684: Infrastructure Investment and Jobs Act", SponsorId = 400100 }
        };

        var votes = new[]
        {
            new Vote { Id = 100, BillId = 2952375 },
            new Vote { Id = 101, BillId = 2900994 },
        };

        var results = new[]
        {
            new VoteResult { Id = 1, LegislatorId = 1, VoteId = 100, VoteType = 1 },
            new VoteResult { Id = 2, LegislatorId = 2, VoteId = 100, VoteType = 2 },

            new VoteResult { Id = 3, LegislatorId = 3, VoteId = 101, VoteType = 1 },
            new VoteResult { Id = 4, LegislatorId = 4, VoteId = 101, VoteType = 1 },
            new VoteResult { Id = 5, LegislatorId = 5, VoteId = 101, VoteType = 2 },
        };

        var (svc, cache) = Create(people, bills, votes, results);
        cache.EnsureLoadedAsync().GetAwaiter().GetResult();

        var billsSummary = svc.GetBillsSummary();

        var bbb = billsSummary.Single(x => x.BillId == 2952375);
        bbb.PrimarySponsor.Should().Be("Rep. John Yarmuth (D-KY-3)");
        bbb.Supporters.Should().Be(1);
        bbb.Opposers.Should().Be(1);

        var infra = billsSummary.Single(x => x.BillId == 2900994);
        infra.PrimarySponsor.Should().Be("—");
        infra.Supporters.Should().Be(2);
        infra.Opposers.Should().Be(1);
    }

    [Fact]
    public void Should_ignore_unknown_vote_type_and_continue()
    {
        var people = Array.Empty<Legislator>();
        var bills = new[] { new Bill { Id = 10, Title = "Bill", SponsorId = null } };
        var votes = new[] { new Vote { Id = 100, BillId = 10 } };
        var results = new[]
        {
            new VoteResult { Id = 1, LegislatorId = 1, VoteId = 100, VoteType = 9 },
            new VoteResult { Id = 2, LegislatorId = 2, VoteId = 100, VoteType = 1 },
        };

        var (svc, cache) = Create(people, bills, votes, results);
        cache.EnsureLoadedAsync().GetAwaiter().GetResult();

        var sum = svc.GetBillsSummary().Single(x => x.BillId == 10);
        sum.Supporters.Should().Be(1);
        sum.Opposers.Should().Be(0);
    }
}
