﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ordos.DataService.Data;
using Ordos.DataService;
using Ordos.IEDService;
using Logger = Ordos.Core.Utilities.Logger;

namespace Ordos.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger.Init();

            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<SystemContext>();
                    DbInitializer.Initialize(context);

                    DatabaseService.Init();
                    SchedulerService.ScheduleCollect(MMSService.GetComtrades);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
