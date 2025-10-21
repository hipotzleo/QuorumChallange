namespace LegalQuorum.Domain.Models
{
    public record Legislator
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }

    public record Bill
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public int? SponsorId { get; init; }
    }

    public record Vote
    {
        public int Id { get; init; }
        public int BillId { get; init; }
    }

    public record VoteResult
    {
        public int Id { get; init; }
        public int LegislatorId { get; init; }
        public int VoteId { get; init; }
        public int VoteType { get; init; }
    }

    public record LegislatorSummaryDto(int LegislatorId, string Name, int Supported, int Opposed);
    public record BillSummaryDto(int BillId, string Title, string PrimarySponsor, int Supporters, int Opposers);
}
