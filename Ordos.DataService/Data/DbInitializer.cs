using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ordos.Core.Models;

namespace Ordos.DataService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SystemContext context)
        {
            context.Database.EnsureCreated();

            ////var ied1 = new Device()
            ////{
            ////    Bay = "Bay1",
            ////    DeviceType = "REL670",
            ////    IPAddress = "192.168.1.1",
            ////    IsConnected = true,
            ////    HasPing = true,
            ////    Name = "IED1",
            ////    Station = "Station1",
            ////    DisturbanceRecordings = new List<DisturbanceRecording>()
            ////};
            ////context.Devices.Add(ied1);

            //var ied1 = context.Devices.First();

            //var dr1 = new DisturbanceRecording()
            //{
            //    Name = "DR3",
            //    DeviceId = ied1.Id,
            //    TriggerTime = DateTime.ParseExact("2015-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            //    Device = ied1,
            //};
            //context.DisturbanceRecordings.Add(dr1);
            //var dr2 = new DisturbanceRecording()
            //{
            //    Name = "DR4",
            //    DeviceId = ied1.Id,
            //    TriggerTime = DateTime.ParseExact("2018-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            //    Device = ied1,
            //};
            //context.DisturbanceRecordings.Add(dr2);

            //context.SaveChanges();

            // Look if DB was seeded;
            if (context.Devices.Any() || context.ConfigurationValues.Any())
                return;   // DB has been seeded

            var configuration = new[]
            {
                new ConfigurationValue()
                {
                    Id = DatabaseService.CompanyNameLabel,
                    Value = @"MyCompany",
                },
            };
            foreach (var item in configuration)
            {
                context.ConfigurationValues.Add(item);
            }
            context.SaveChanges();
        }
    }
}
