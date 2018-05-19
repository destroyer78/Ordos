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
                return;   // DB has been seeded



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
