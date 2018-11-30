using CommonModels.Config;
using DataService.Entities;
using Messages;
using Microsoft.Extensions.Options;
using QueueService.Builders;
using QueueService.Interfaces;
namespace TestOrchestrator.Services
{
    public interface IQueues
    {
        void EnqueueTests(Job job);
        void DeleteQueue(int jobId);
    }

    public class Queues : IQueues
    {
        private readonly IOptions<QueueSettings> _settings;

        public Queues(IOptions<QueueSettings> settings)
        {
            _settings = settings;
        }

        public void EnqueueTests(Job job)
        {
            string queueName = $"{job.JobId}";
            IQueueBuilder queueBuilder = new RabbitBuilder();

            using (ISender sender = queueBuilder
                .ConfigureTransport(_settings.Value.Server, _settings.Value.Vhost, _settings.Value.Username, _settings.Value.Password)
                .ISendTo(queueName)
                .Build())
            {
                foreach(TestRequest request in job.TestRequests)
                {
                    EnqueueTest(sender, request);
                }
            }
        }

        public void DeleteQueue(int jobId)
        {
            string queueName = $"{jobId}";
            IQueueBuilder queueBuilder = new RabbitBuilder();

            using (ISender sender = queueBuilder
                .ConfigureTransport(_settings.Value.Server, _settings.Value.Vhost, _settings.Value.Username, _settings.Value.Password)
                .ISendTo(queueName)
                .Build())
            {
                sender.DeleteQueue();
            }
        }

        private void EnqueueTest(ISender sender, TestRequest request)
        {
            RunTest message = new RunTest
            {
                TestRequestId  = request.TestRequestId,
                FullName = request.TestName
            };

            sender.Send(message);
        }
    }
}