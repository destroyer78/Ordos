using FluentScheduler;
using System;

namespace Ordos.IEDService.Services
{
    public static class SchedulerService
    {
        private static Registry registry;

        public static void ScheduleCollect(Action collectMethod)
        {
            registry = new Registry();
            registry.Schedule(collectMethod).NonReentrant().ToRunNow().AndEvery(60).Seconds();
            JobManager.Initialize(registry);
        }        
    }
}