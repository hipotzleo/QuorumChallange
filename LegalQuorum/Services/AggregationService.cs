using LegalQuorum.Domain.Models;
using LegalQuorum.Infra.Cache;

namespace LegalQuorum.Services
{
    public sealed class AggregationService
    {
        private readonly DataCache _cache;
        public AggregationService(DataCache cache) => _cache = cache;

        public IReadOnlyList<LegislatorSummaryDto> GetLegislatorsSummary()
        {
            _cache.EnsureLoadedAsync().GetAwaiter().GetResult();
            var (people, bills, votes, results) = _cache.Snapshot();

            var votesById = votes.ToDictionary(v => v.Id);
            var supportByLegislator = new Dictionary<int, int>();
            var opposeByLegislator = new Dictionary<int, int>();

            foreach (var vr in results)
            {
                if (!votesById.TryGetValue(vr.VoteId, out _)) continue;

                if (vr.VoteType == 1)
                    supportByLegislator[vr.LegislatorId] = supportByLegislator.GetValueOrDefault(vr.LegislatorId) + 1;
                else if (vr.VoteType == 2)
                    opposeByLegislator[vr.LegislatorId] = opposeByLegislator.GetValueOrDefault(vr.LegislatorId) + 1;
            }

            var list = new List<LegislatorSummaryDto>(people.Count);
            foreach (var p in people)
            {
                supportByLegislator.TryGetValue(p.Id, out var sup);
                opposeByLegislator.TryGetValue(p.Id, out var opp);
                list.Add(new LegislatorSummaryDto(p.Id, p.Name, sup, opp));
            }

            return list.OrderByDescending(x => x.Supported).ThenBy(x => x.Name).ToList();
        }

        public IReadOnlyList<BillSummaryDto> GetBillsSummary()
        {
            _cache.EnsureLoadedAsync().GetAwaiter().GetResult();
            var (people, bills, votes, results) = _cache.Snapshot();

            var votesById = votes.ToDictionary(v => v.Id);
            var peopleById = people.ToDictionary(p => p.Id);

            var supportByBill = new Dictionary<int, int>();
            var opposeByBill = new Dictionary<int, int>();

            foreach (var vr in results)
            {
                if (!votesById.TryGetValue(vr.VoteId, out var vote)) continue;
                var billId = vote.BillId;

                if (vr.VoteType == 1)
                    supportByBill[billId] = supportByBill.GetValueOrDefault(billId) + 1;
                else if (vr.VoteType == 2)
                    opposeByBill[billId] = opposeByBill.GetValueOrDefault(billId) + 1;
            }

            var list = new List<BillSummaryDto>(bills.Count);
            foreach (var b in bills)
            {
                supportByBill.TryGetValue(b.Id, out var sup);
                opposeByBill.TryGetValue(b.Id, out var opp);

                var sponsorName =
                    b.SponsorId.HasValue && peopleById.TryGetValue(b.SponsorId.Value, out var sponsor)
                    ? sponsor.Name
                    : "—";

                list.Add(new BillSummaryDto(b.Id, b.Title, sponsorName, sup, opp));
            }

            return list.OrderByDescending(x => x.Supporters).ThenBy(x => x.Title).ToList();
        }
    }
}