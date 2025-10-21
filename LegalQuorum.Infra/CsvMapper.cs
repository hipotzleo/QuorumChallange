using CsvHelper.Configuration;
using LegalQuorum.Domain.Models;

public sealed class LegislatorMap : ClassMap<Legislator>
{
    public LegislatorMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.Name).Name("name");
    }
}

public sealed class BillMap : ClassMap<Bill>
{
    public BillMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.Title).Name("title");
        Map(m => m.SponsorId).Name("sponsor_id");
    }
}

public sealed class VoteMap : ClassMap<Vote>
{
    public VoteMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.BillId).Name("bill_id");
    }
}

public sealed class VoteResultMap : ClassMap<VoteResult>
{
    public VoteResultMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.LegislatorId).Name("legislator_id");
        Map(m => m.VoteId).Name("vote_id");
        Map(m => m.VoteType).Name("vote_type");
    }
}
