using System.Globalization;
using CsvHelper.Configuration;
using LegalQuorum.Domain.Models;
using LegalQuorum.Infra;
using LegalQuorum.Infra.Cache;
using LegalQuorum.Infra.CsvSetup;
using LegalQuorum.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

builder.Services.Configure<DataPaths>(builder.Configuration.GetSection("DataPaths"));

builder.Services.AddCsvInfrastructure(new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    DetectDelimiter = true,
    TrimOptions = TrimOptions.Trim,
    PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),
    HeaderValidated = null,
    MissingFieldFound = null
});

builder.Services.AddSingleton<DataCache>();
builder.Services.AddSingleton<AggregationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.MapRazorPages();

app.MapGet("/api/legislators/summary", (AggregationService svc) => Results.Ok(svc.GetLegislatorsSummary()));
app.MapGet("/api/bills/summary", (AggregationService svc) => Results.Ok(svc.GetBillsSummary()));

app.Run();