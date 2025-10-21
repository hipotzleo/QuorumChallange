using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace LegalQuorum.Infra;

public sealed class CsvReaderService<T> : ICsvReader<T>
{
    private readonly CsvConfiguration _config;
    private readonly ILogger<CsvReaderService<T>> _logger;

    public CsvReaderService(ILogger<CsvReaderService<T>> logger, CsvConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async IAsyncEnumerable<T> ReadAsync(string path, CancellationToken ct = default)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, _config);

        RegisterMaps(csv);

        var rowNumber = 0;
        if (await csv.ReadAsync())
        {
            csv.ReadHeader();
            rowNumber = 1;
        }

        while (await csv.ReadAsync())
        {
            rowNumber++;
            T? record = default;

            try
            {
                record = csv.GetRecord<T>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CSV: Invalid Line ignored. File={File} Row={Row}", path, rowNumber);
            }

            if (record is not null)
                yield return record;
        }
    }

    private static void RegisterMaps(CsvReader csv)
    {
        var ctx = csv.Context;
        ctx.RegisterClassMap<LegislatorMap>();
        ctx.RegisterClassMap<BillMap>();
        ctx.RegisterClassMap<VoteMap>();
        ctx.RegisterClassMap<VoteResultMap>();
    }
}
