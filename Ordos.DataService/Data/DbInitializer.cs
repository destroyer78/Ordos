using System.Linq;
using Ordos.Core.Models;

namespace Ordos.DataService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SystemContext context)
        {
            context.Database.EnsureCreated();

            // Look if DB was seeded;
            if (context.Devices.Any() || context.ConfigurationValues.Any())
                //if (context.Devices.Any() || context.DeviceTypes.Any() || context.ConfigurationValues.Any())
                return;   // DB has been seeded

            //var deviceTypes = new[]
            //{
            //    new DeviceType()
            //    {
            //        ComtradePath = "drec",
            //        FtpPassword = "Administrator",
            //        FtpUsername = "Administrator",
            //        Manufacturer = "ABB",
            //        Model = "REx670",
            //    },
            //    new DeviceType()
            //    {
            //        ComtradePath = "COMTRADE",
            //        FtpUsername = "ADMINISTRATOR",
            //        FtpPassword = "remote0004",
            //        Manufacturer = "ABB",
            //        Model = "REx615",
            //    },
            //};
            //foreach (var item in deviceTypes)
            //{
            //    context.DeviceTypes.Add(item);
            //}
            //context.SaveChanges();

            var configuration = new[]
            {
                new ConfigurationValue()
                {
                    Id = "CompanyName",
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
