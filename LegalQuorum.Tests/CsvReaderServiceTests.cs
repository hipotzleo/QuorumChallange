using System.Globalization;
using CsvHelper.Configuration;
using FluentAssertions;
using LegalQuorum.Domain.Models;
using LegalQuorum.Infra;
using LegalQuorum.Tests.Helpers;

namespace LegalQuorum.Tests;

public class CsvReaderServiceTests
{
    private static CsvConfiguration Config() => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        DetectDelimiter = true,
        TrimOptions = TrimOptions.Trim,
        PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),
        HeaderValidated = null,
        MissingFieldFound = null
    };

    [Fact]
    public async Task Reads_valid_bills_ok()
    {
        var csv = "id,title,sponsor_id\n1,Test Bill,10\n2,Another,11\n";
        var path = Helpers.TempFile.Create(csv);
        var logger = new TestLogger<CsvReaderService<Bill>>();
        var svc = new CsvReaderService<Bill>(logger, Config());

        var list = new List<Bill>();
        await foreach (var b in svc.ReadAsync(path))
            list.Add(b);

        list.Should().HaveCount(2);
        list[0].Id.Should().Be(1);
        list[0].SponsorId.Should().Be(10);
        logger.CountWarnings().Should().Be(0);
        logger.CountErrors().Should().Be(0);
    }

    [Fact]
    public async Task Invalid_row_is_logged_and_skipped()
    {
        var csv = "id,title,sponsor_id\n1,Ok,10\n2,Bad,not_an_int\n3,Ok2,12\n";
        var path = Helpers.TempFile.Create(csv);
        var logger = new TestLogger<CsvReaderService<Bill>>();
        var svc = new CsvReaderService<Bill>(logger, Config());

        var list = new List<Bill>();
        await foreach (var b in svc.ReadAsync(path))
            list.Add(b);

        list.Should().HaveCount(2);
        list.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 3 });
        logger.CountWarnings().Should().BeGreaterThanOrEqualTo(1);
        logger.CountErrors().Should().Be(0);
    }

    [Fact]
    public async Task Header_case_insensitive_should_work()
    {
        var csv = "ID,Title,SPONSOR_ID\n10,Uppercase,123\n";
        var path = Helpers.TempFile.Create(csv);
        var logger = new TestLogger<CsvReaderService<Bill>>();
        var svc = new CsvReaderService<Bill>(logger, Config());

        var list = new List<Bill>();
        await foreach (var b in svc.ReadAsync(path))
            list.Add(b);

        list.Should().ContainSingle();
        list[0].Id.Should().Be(10);
        list[0].SponsorId.Should().Be(123);
    }
}
