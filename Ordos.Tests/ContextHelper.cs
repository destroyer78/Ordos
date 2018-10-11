using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;

namespace Ordos.Tests
{
    public static class ContextHelper
    {
        public static SystemContext GetContextWithData()
        {
            var options = new DbContextOptionsBuilder<SystemContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new SystemContext(options);

            var ied1 = new Device()
            {
                Bay = "Bay1",
                DeviceType = "REL670",
                IPAddress = "192.168.1.1",
                Id = 1,
                IsConnected = true,
                HasPing = true,
                Name = "IED1",
                Station = "Station1",
                DisturbanceRecordings = new List<DisturbanceRecording>()
            };
            context.Devices.Add(ied1);

            var dr1 = new DisturbanceRecording()
            {
                Id = 1,
                Name = "DR1",
                DeviceId = 1,
                TriggerTime = DateTime.ParseExact("2017-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Device = ied1,
            };
            context.DisturbanceRecordings.Add(dr1);

            context.SaveChanges();

            return context;
        }
    }
}
