using CommonModels.Config;
using DataService.Services;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueService.Builders;
using QueueService.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestOrchestrator.Services
{
    public class QueueSubscriberService : IHostedService, IDisposable
    {
        private readonly IOptions<QueueSettings> _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private IReceiver _receiver;

        public static readonly string RESPONSE_QUEUE_NAME = "TestResults";

        public QueueSubscriberService(IOptions<QueueSettings> settings, IServiceProvider serviceProvider, ILogger<QueueSubscriberService> logger)
        {
            _settings = settings;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            IQueueBuilder queueBuilder = new RabbitBuilder();
            _receiver = queueBuilder.ConfigureTransport(_settings.Value.Server, _settings.Value.Vhost, _settings.Value.Username, _settings.Value.Password)
                .IReceiveFrom(RESPONSE_QUEUE_NAME)
                .IReceiveForever()
                .Build();

            _receiver.Receive<TestResult>(ReceiveMessage);


            return Task.CompletedTask;
        }

        private void ReceiveMessage(TestResult testResult)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var jobService =
                    scope.ServiceProvider
                        .GetRequiredService<IJobService>();

                try
                {
                    jobService.CreateTestResponse(testResult.TestRequestId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error saving test response with TestRequestId {0}", testResult.TestRequestId);
                }
                
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _receiver.Dispose();

            return Task.CompletedTask;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _receiver?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~QueueSubscriberService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
