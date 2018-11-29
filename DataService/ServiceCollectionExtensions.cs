using DataService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataService
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataService(this IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddDbContext<DataContext>((options) => {
                options.UseSqlServer(connectionString);
            });

            serviceCollection.AddTransient<IJobService, JobService>();
        }
    }
}
