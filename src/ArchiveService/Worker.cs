using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private StdSchedulerFactory _schedulerFactory;
        private CancellationToken _stopppingToken;
        private IScheduler _scheduler;
        private IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartJobs();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }
        }

        protected async Task StartJobs()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<DownloadJob>();
            serviceCollection.AddSingleton<IBookBlobService>(new BookBlobService(_configuration.GetSection("BlobConnectionString").Value));
            serviceCollection.AddSingleton<IConfiguration>(_configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _schedulerFactory = new StdSchedulerFactory();

            _scheduler = await _schedulerFactory.GetScheduler();
            _scheduler.JobFactory = new DownloadJobFactory(serviceProvider);

            await _scheduler.Start();

            IJobDetail job1 = JobBuilder.Create<DownloadJob>()
                .WithIdentity("job1", "gtoup")
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .WithIdentity("trigger_20_sec", "group")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(_configuration.GetValue<int>("DownloadSchedulerInterval"))
                    .RepeatForever())
            .Build();


            await _scheduler.ScheduleJob(job1, trigger1, _stopppingToken);
        }
    }
}
