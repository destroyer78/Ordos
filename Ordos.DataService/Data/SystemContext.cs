using Ordos.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;

namespace Ordos.DataService.Data
{
    public class SystemContext : DbContext
    {
        public static string PSQLConnectionString => Environment.GetEnvironmentVariable("ORDOS_CONNECTION_STRING");

        //public static Guid guid = new Guid();

        public SystemContext()
        {
        }

        public SystemContext(DbContextOptions<SystemContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DisturbanceRecording> DisturbanceRecordings { get; set; }
        public DbSet<DRFile> DRFiles { get; set; }
        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(PSQLConnectionString);
            // optionsBuilder.UseInMemoryDatabase(guid.ToString());
        }
    }
}
