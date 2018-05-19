using Ordos.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ordos.DataService.Data
{
    public class SystemContext : DbContext
    {
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
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Ordos;ConnectRetryCount=0;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
