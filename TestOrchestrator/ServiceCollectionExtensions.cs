using Microsoft.Extensions.DependencyInjection;
using TestOrchestrator.Services;

namespace TestOrchestrator
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupServices(this IServiceCollection self)
        {
            self.AddTransient<IQueues, Queues>();
        }
    }
}
