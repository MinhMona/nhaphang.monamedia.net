using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NhapHangV2.Interface.Services.BackgroundServices;

namespace NhapHangV2.Service.Services.BackgroundServices
{
    public class RuntimeBackgroundService: BackgroundService
    {
        public IBackgroundTaskQueue TaskQueue { get; }

        public RuntimeBackgroundService(IBackgroundTaskQueue taskQueue)
        {
            TaskQueue = taskQueue;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
