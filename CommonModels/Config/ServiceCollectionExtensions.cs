using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommonModels.Config
{
    public static class ServiceCollectionExtensions
    {
        public static void UseQueueConfig(this IServiceCollection self, IConfiguration configuration)
        {
            self.Configure<QueueSettings>(configuration.GetSection("QueueSettings"));
        }
    }
}
