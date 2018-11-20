using Ordos.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ordos.DataService.Data
{
    public class SystemContext : DbContext
    {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        public static string PSQLConnectionString =>
            "Host=localhost;Port=5432;Username=postgres;Password=password1;Database=postgres;";


        public static Guid guid = new Guid();

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
