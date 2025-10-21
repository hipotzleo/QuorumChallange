using CsvHelper.Configuration;
using LegalQuorum.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LegalQuorum.Infra
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCsvInfrastructure(this IServiceCollection services, CsvConfiguration config)
        {
            services.AddSingleton(config);
            services.AddSingleton<ICsvReader<Legislator>, CsvReaderService<Legislator>>();
            services.AddSingleton<ICsvReader<Bill>, CsvReaderService<Bill>>();
            services.AddSingleton<ICsvReader<Vote>, CsvReaderService<Vote>>();
            services.AddSingleton<ICsvReader<VoteResult>, CsvReaderService<VoteResult>>();
            return services;
        }
    }
}
