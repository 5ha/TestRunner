using CommonModels.Config;
using DataService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestOrchestrator.Models;
using TestOrchestrator.Services;

namespace TestOrchestrator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.UseQueueConfig(Configuration);

            services.Configure<TestAnalyserSettings>(Configuration.GetSection("TestAnalyserSettings"));

            services.Configure<ComposerSettings>(Configuration.GetSection("ComposerSettings"));

            services.AddDataService(Configuration.GetConnectionString("TestRunnerDatabase"));

            services.AddHostedService<QueueSubscriberService>();

            services.SetupServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
