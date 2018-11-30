using DataService.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DataService.Services
{
    public interface IJobService
    {
        Job CreateJob(string description, List<string> tests);
        TestResponse CreateTestResponse(int testRequestId);
    }

    public class JobService : IJobService
    {
        private readonly ILogger<JobService> _logger;
        private readonly DataContext _context;

        public JobService(ILogger<JobService> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public Job CreateJob(string description, List<string> tests)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("Description", nameof(description));
            }

            Job job = new Job
            {
                DateCreated = DateTime.UtcNow,
                Description = description,
                TestRequests = new List<TestRequest>()
            };

            foreach (string test in tests)
            {
                TestRequest request = new TestRequest
                {
                    TestName = test
                };

                job.TestRequests.Add(request);
            }

            _context.Jobs.Add(job);
            _context.SaveChanges();

            return job;
        }

        public TestResponse CreateTestResponse(int testRequestId)
        {
            TestResponse response = new TestResponse
            {
                TestRequestId = testRequestId,
                DateCreated = DateTime.UtcNow
            };

            _context.TestResponses.Add(response);
            _context.SaveChanges();

            return response;
        }
    }
}
